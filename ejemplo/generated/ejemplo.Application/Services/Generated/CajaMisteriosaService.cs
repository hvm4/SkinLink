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

    public async Task ApplyCajaMisteriosaRelationsAsync(CajaMisteriosa entity, IReadOnlyList<ProbabilidadDrop> p_probabilidades)
    {
        if (p_probabilidades.Count < 1 || p_probabilidades.Count > 500) throw new ModelException("Invalid number of Probabilidades rows.");
        foreach (var removed in entity.Probabilidades.Where(child => !p_probabilidades.Contains(child)).ToList()) entity.Probabilidades.Remove(removed);
        foreach (var child in p_probabilidades)
        {
            child.Caja = entity;
            if (!entity.Probabilidades.Contains(child)) entity.Probabilidades.Add(child);
        }
    }

    public async Task<List<ProbabilidadDrop>> PrepareCajaMisteriosaProbabilidadesAsync(CajaMisteriosa? parent, IReadOnlyList<(int Id, ProbabilidadDrop Values, int? ActivoRecompensaId)> rows)
    {
        if (rows.Count < 1 || rows.Count > 500) throw new ModelException("Invalid number of Probabilidades rows.");
        var seen = new HashSet<int>();
        var result = new List<ProbabilidadDrop>();
        foreach (var row in rows)
        {
            var existing = !EqualityComparer<int>.Default.Equals(row.Id, default!);
            if (existing && (parent is null || !seen.Add(row.Id) || !parent.Probabilidades.Any(child => child.Id.Equals(row.Id))))
                throw new ModelException("The row does not belong to this record or is duplicated.");
            var child = existing ? parent!.Probabilidades.Single(item => item.Id.Equals(row.Id)) : new ProbabilidadDrop();
            child.PorcentajeDrop = row.Values.PorcentajeDrop;
            if (row.ActivoRecompensaId is null) throw new ModelException("ProbabilidadDrop.ActivoRecompensa is required.");
            child.ActivoRecompensa = row.ActivoRecompensaId is int selectedActivoRecompensa
                ? await _unitOfWork.ActivoDigitalRepository.ReadAsync(selectedActivoRecompensa) ?? throw new ModelException("ActivoDigital not found for ProbabilidadDrop.ActivoRecompensa.") : null!;
            result.Add(child);
        }
        return result;
    }

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

    public async Task<IReadOnlyList<CajaMisteriosa>> FiltrarCajasActivasAsync()
    {
        return await _repository.FiltrarCajasActivasAsync();
    }

    public async Task<IReadOnlyList<CajaMisteriosa>> FiltrarCajasPorPrecioAperturaAsync(double p_precioMinimo, double p_precioMaximo)
    {
        return await _repository.FiltrarCajasPorPrecioAperturaAsync(p_precioMinimo, p_precioMaximo);
    }

}
