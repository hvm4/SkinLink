using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class InventarioPrivadoService : ServiceEntity<InventarioPrivado>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventarioPrivadoRepository _repository;

    public InventarioPrivadoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public InventarioPrivadoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private InventarioPrivadoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.InventarioPrivadoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.InventarioPrivadoRepository;
    }

    public Task<InventarioPrivado> AddInventarioPrivadoDefaultAsync(InventarioPrivado entity) =>
        AddDefaultAsync(entity);

    public Task DeleteInventarioPrivadoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<InventarioPrivado?> ReadOIDInventarioPrivadoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<InventarioPrivado>> ReadAllInventarioPrivadoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<InventarioPrivado> ModifierInventarioPrivadoDefaultAsync(InventarioPrivado entity) =>
        ModifierDefaultAsync(entity);

    public async Task<InventarioPrivado> NewAsync(Moneda p_valorEstimadoTotal, int p_usuario_oid, IReadOnlyList<ActivoDigital> p_activos)
    {
        Usuario usuarioEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for InventarioPrivado.Usuario.");

        InventarioPrivado entity = new InventarioPrivado
        {
            ValorEstimadoTotal = p_valorEstimadoTotal,
            Usuario = usuarioEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        InventarioPrivado entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("InventarioPrivado not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, Moneda p_valorEstimadoTotal)
    {
        InventarioPrivado entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("InventarioPrivado not found.");
        entity.ValorEstimadoTotal = p_valorEstimadoTotal;
        await _repository.SaveAsync(entity);
    }

    public async Task<InventarioPrivado?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<InventarioPrivado>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
