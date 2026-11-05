using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class MonederoService : ServiceEntity<Monedero>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMonederoRepository _repository;

    public MonederoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public MonederoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private MonederoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.MonederoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.MonederoRepository;
    }

    public Task<Monedero> AddMonederoDefaultAsync(Monedero entity) =>
        AddDefaultAsync(entity);

    public Task DeleteMonederoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Monedero?> ReadOIDMonederoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Monedero>> ReadAllMonederoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Monedero> ModifierMonederoDefaultAsync(Monedero entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Monedero> NewAsync(Moneda p_saldoDisponible, Moneda p_saldoPendiente, int p_usuario_oid)
    {
        Usuario usuarioEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Monedero.Usuario.");

        Monedero entity = new Monedero
        {
            SaldoDisponible = p_saldoDisponible,
            SaldoPendiente = p_saldoPendiente,
            Usuario = usuarioEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Monedero entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Monedero not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, Moneda p_saldoDisponible, Moneda p_saldoPendiente)
    {
        Monedero entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Monedero not found.");
        entity.SaldoDisponible = p_saldoDisponible;
        entity.SaldoPendiente = p_saldoPendiente;
        await _repository.SaveAsync(entity);
    }

    public async Task<Monedero?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Monedero>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
