using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class VentaService : ServiceEntity<Venta>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVentaRepository _repository;

    public VentaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public VentaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private VentaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.VentaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.VentaRepository;
    }

    public Task<Venta> AddVentaDefaultAsync(Venta entity) =>
        AddDefaultAsync(entity);

    public Task DeleteVentaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Venta?> ReadOIDVentaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Venta>> ReadAllVentaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Venta> ModifierVentaDefaultAsync(Venta entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Venta> NewAsync(Moneda p_precioVenta, DateTime p_fechaPublicacion, EstadoVenta p_estadoVenta, int p_vendedor_oid, int p_comprador_oid, int p_activoDigital_oid, int p_transaccionCobro_oid, int p_transaccionPago_oid)
    {
        Usuario vendedorEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_vendedor_oid)
            ?? throw new ModelException("Usuario not found for Venta.Vendedor.");

        ActivoDigital activoDigitalEntity = await _unitOfWork.ActivoDigitalRepository.ReadAsync(p_activoDigital_oid)
            ?? throw new ModelException("ActivoDigital not found for Venta.ActivoDigital.");

        Venta entity = new Venta
        {
            PrecioVenta = p_precioVenta,
            FechaPublicacion = p_fechaPublicacion,
            EstadoVenta = p_estadoVenta,
            Vendedor = vendedorEntity,
            ActivoDigital = activoDigitalEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Venta entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Venta not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, Moneda p_precioVenta, DateTime p_fechaPublicacion, EstadoVenta p_estadoVenta, int p_activoDigital_oid)
    {
        Venta entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Venta not found.");
        entity.PrecioVenta = p_precioVenta;
        entity.FechaPublicacion = p_fechaPublicacion;
        entity.EstadoVenta = p_estadoVenta;
        await _repository.SaveAsync(entity);
    }

    public async Task<Venta?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Venta>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task<IReadOnlyList<Venta>> FiltrarVentasPorEstadoAsync(EstadoVenta p_estado)
    {
        return await _repository.FiltrarVentasPorEstadoAsync(p_estado);
    }

    public async Task<IReadOnlyList<Venta>> FiltrarVentasPorPrecioMaximoAsync(double p_precioMaximo)
    {
        return await _repository.FiltrarVentasPorPrecioMaximoAsync(p_precioMaximo);
    }

}
