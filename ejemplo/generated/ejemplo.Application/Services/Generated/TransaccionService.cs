using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class TransaccionService : ServiceEntity<Transaccion>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransaccionRepository _repository;

    public TransaccionService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public TransaccionService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private TransaccionService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.TransaccionRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.TransaccionRepository;
    }

    public Task<Transaccion> AddTransaccionDefaultAsync(Transaccion entity) =>
        AddDefaultAsync(entity);

    public Task DeleteTransaccionDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Transaccion?> ReadOIDTransaccionDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Transaccion>> ReadAllTransaccionDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Transaccion> ModifierTransaccionDefaultAsync(Transaccion entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Transaccion> NewAsync(TipoTransaccion p_tipo, Moneda p_monto, DateTime p_fecha, string p_concepto, int p_monedero_oid)
    {
        Monedero monederoEntity = await _unitOfWork.MonederoRepository.ReadAsync(p_monedero_oid)
            ?? throw new ModelException("Monedero not found for Transaccion.Monedero.");

        Transaccion entity = new Transaccion
        {
            Tipo = p_tipo,
            Monto = p_monto,
            Fecha = p_fecha,
            Concepto = p_concepto,
            Monedero = monederoEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Transaccion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Transaccion not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, TipoTransaccion p_tipo, Moneda p_monto, DateTime p_fecha, string p_concepto)
    {
        Transaccion entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Transaccion not found.");
        entity.Tipo = p_tipo;
        entity.Monto = p_monto;
        entity.Fecha = p_fecha;
        entity.Concepto = p_concepto;
        await _repository.SaveAsync(entity);
    }

    public async Task<Transaccion?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Transaccion>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorTipoAsync(TipoTransaccion p_tipo)
    {
        return await _repository.FiltrarTransaccionesPorTipoAsync(p_tipo);
    }

    public async Task<IReadOnlyList<Transaccion>> FiltrarTransaccionesPorRangoFechasAsync(DateTime p_fechaInicio, DateTime p_fechaFin)
    {
        return await _repository.FiltrarTransaccionesPorRangoFechasAsync(p_fechaInicio, p_fechaFin);
    }

}
