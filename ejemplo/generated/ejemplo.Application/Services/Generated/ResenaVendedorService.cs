using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class ResenaVendedorService : ServiceEntity<ResenaVendedor>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IResenaVendedorRepository _repository;

    public ResenaVendedorService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public ResenaVendedorService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private ResenaVendedorService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.ResenaVendedorRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.ResenaVendedorRepository;
    }

    public Task<ResenaVendedor> AddResenaVendedorDefaultAsync(ResenaVendedor entity) =>
        AddDefaultAsync(entity);

    public Task DeleteResenaVendedorDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<ResenaVendedor?> ReadOIDResenaVendedorDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<ResenaVendedor>> ReadAllResenaVendedorDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<ResenaVendedor> ModifierResenaVendedorDefaultAsync(ResenaVendedor entity) =>
        ModifierDefaultAsync(entity);

    public async Task<ResenaVendedor> NewAsync(int p_puntuacion, string p_comentario, DateTime p_fecha, bool p_oculta, int p_comprador_oid, int p_vendedor_oid)
    {
        Usuario compradorEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_comprador_oid)
            ?? throw new ModelException("Usuario not found for ResenaVendedor.Comprador.");

        Usuario vendedorEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_vendedor_oid)
            ?? throw new ModelException("Usuario not found for ResenaVendedor.Vendedor.");

        ResenaVendedor entity = new ResenaVendedor
        {
            Puntuacion = p_puntuacion,
            Comentario = p_comentario,
            Fecha = p_fecha,
            Oculta = p_oculta,
            Comprador = compradorEntity,
            Vendedor = vendedorEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        ResenaVendedor entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ResenaVendedor not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, int p_puntuacion, string p_comentario, DateTime p_fecha, bool p_oculta)
    {
        ResenaVendedor entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ResenaVendedor not found.");
        entity.Puntuacion = p_puntuacion;
        entity.Comentario = p_comentario;
        entity.Fecha = p_fecha;
        entity.Oculta = p_oculta;
        await _repository.SaveAsync(entity);
    }

    public async Task<ResenaVendedor?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<ResenaVendedor>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task<IReadOnlyList<ResenaVendedor>> FiltrarResenasVisiblesPorVendedorAsync(int p_vendedorId)
    {
        return await _repository.FiltrarResenasVisiblesPorVendedorAsync(p_vendedorId);
    }

}
