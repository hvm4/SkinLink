using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class IntercambioService : ServiceEntity<Intercambio>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntercambioRepository _repository;

    public IntercambioService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public IntercambioService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private IntercambioService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.IntercambioRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.IntercambioRepository;
    }

    public Task<Intercambio> AddIntercambioDefaultAsync(Intercambio entity) =>
        AddDefaultAsync(entity);

    public Task DeleteIntercambioDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Intercambio?> ReadOIDIntercambioDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Intercambio>> ReadAllIntercambioDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Intercambio> ModifierIntercambioDefaultAsync(Intercambio entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Intercambio> NewAsync(EstadoIntercambio p_estado, DateTime p_fechaCreacion, int p_proponente_oid, int p_receptor_oid)
    {
        Usuario proponenteEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_proponente_oid)
            ?? throw new ModelException("Usuario not found for Intercambio.Proponente.");

        Usuario receptorEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_receptor_oid)
            ?? throw new ModelException("Usuario not found for Intercambio.Receptor.");

        Intercambio entity = new Intercambio
        {
            Estado = p_estado,
            FechaCreacion = p_fechaCreacion,
            Proponente = proponenteEntity,
            Receptor = receptorEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Intercambio entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Intercambio not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, EstadoIntercambio p_estado, DateTime p_fechaCreacion)
    {
        Intercambio entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Intercambio not found.");
        entity.Estado = p_estado;
        entity.FechaCreacion = p_fechaCreacion;
        await _repository.SaveAsync(entity);
    }

    public async Task<Intercambio?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Intercambio>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
