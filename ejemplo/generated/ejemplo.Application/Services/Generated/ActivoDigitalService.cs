using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class ActivoDigitalService : ServiceEntity<ActivoDigital>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivoDigitalRepository _repository;

    public ActivoDigitalService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public ActivoDigitalService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private ActivoDigitalService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.ActivoDigitalRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.ActivoDigitalRepository;
    }

    public Task<ActivoDigital> AddActivoDigitalDefaultAsync(ActivoDigital entity) =>
        AddDefaultAsync(entity);

    public Task DeleteActivoDigitalDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<ActivoDigital?> ReadOIDActivoDigitalDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<ActivoDigital>> ReadAllActivoDigitalDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<ActivoDigital> ModifierActivoDigitalDefaultAsync(ActivoDigital entity) =>
        ModifierDefaultAsync(entity);

    public async Task<ActivoDigital> NewAsync(string p_nombre, string p_coleccion, Moneda p_precioEstimado, AtributosSkin p_atributos, bool p_enVenta, string p_identificadorUnico, int p_inventario_oid)
    {
        InventarioPrivado inventarioEntity = await _unitOfWork.InventarioPrivadoRepository.ReadAsync(p_inventario_oid)
            ?? throw new ModelException("InventarioPrivado not found for ActivoDigital.Inventario.");

        ActivoDigital entity = new ActivoDigital
        {
            Nombre = p_nombre,
            Coleccion = p_coleccion,
            PrecioEstimado = p_precioEstimado,
            Atributos = p_atributos,
            EnVenta = p_enVenta,
            IdentificadorUnico = p_identificadorUnico,
            Inventario = inventarioEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        ActivoDigital entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ActivoDigital not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombre, string p_coleccion, Moneda p_precioEstimado, AtributosSkin p_atributos, bool p_enVenta, string p_identificadorUnico)
    {
        ActivoDigital entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ActivoDigital not found.");
        entity.Nombre = p_nombre;
        entity.Coleccion = p_coleccion;
        entity.PrecioEstimado = p_precioEstimado;
        entity.Atributos = p_atributos;
        entity.EnVenta = p_enVenta;
        entity.IdentificadorUnico = p_identificadorUnico;
        await _repository.SaveAsync(entity);
    }

    public async Task<ActivoDigital?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<ActivoDigital>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorRarezaAsync(Rareza p_rareza)
    {
        return await _repository.FiltrarActivosPorRarezaAsync(p_rareza);
    }

    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorColeccionAsync(string p_coleccion)
    {
        return await _repository.FiltrarActivosPorColeccionAsync(p_coleccion);
    }

    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosPorCondicionAsync(Condicion p_condicion)
    {
        return await _repository.FiltrarActivosPorCondicionAsync(p_condicion);
    }

    public async Task<IReadOnlyList<ActivoDigital>> FiltrarActivosEnVentaAsync(int p_usuarioId, bool p_enVenta)
    {
        return await _repository.FiltrarActivosEnVentaAsync(p_usuarioId, p_enVenta);
    }

}
