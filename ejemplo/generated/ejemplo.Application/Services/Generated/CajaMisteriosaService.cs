using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class CajaMisteriosaService : ServiceEntity<CajaMisteriosa>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICajaMisteriosaRepository _repository;

    public CajaMisteriosaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public CajaMisteriosaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private CajaMisteriosaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.CajaMisteriosaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.CajaMisteriosaRepository;
    }

    public Task<CajaMisteriosa> AddCajaMisteriosaDefaultAsync(CajaMisteriosa entity) =>
        AddDefaultAsync(entity);

    public Task DeleteCajaMisteriosaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<CajaMisteriosa?> ReadOIDCajaMisteriosaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<CajaMisteriosa>> ReadAllCajaMisteriosaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<CajaMisteriosa> ModifierCajaMisteriosaDefaultAsync(CajaMisteriosa entity) =>
        ModifierDefaultAsync(entity);

    public async Task<CajaMisteriosa> NewAsync(string p_nombre, Moneda p_precioApertura, bool p_activa, IReadOnlyList<ProbabilidadDrop> p_probabilidades)
    {
        CajaMisteriosa entity = new CajaMisteriosa
        {
            Nombre = p_nombre,
            PrecioApertura = p_precioApertura,
            Activa = p_activa,
            Probabilidades = p_probabilidades.ToList(),
        };

        foreach (var cajaMisteriosaProbabilidadDrop in entity.Probabilidades)
        {
            cajaMisteriosaProbabilidadDrop.Caja = entity;
        }

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        CajaMisteriosa entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("CajaMisteriosa not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombre, Moneda p_precioApertura, bool p_activa, IReadOnlyList<ProbabilidadDrop> p_probabilidades)
    {
        CajaMisteriosa entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("CajaMisteriosa not found.");
        entity.Nombre = p_nombre;
        entity.PrecioApertura = p_precioApertura;
        entity.Activa = p_activa;
        await _repository.SaveAsync(entity);
    }

    public async Task<CajaMisteriosa?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<CajaMisteriosa>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
