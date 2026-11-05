using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class FavoritoService : ServiceEntity<Favorito>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFavoritoRepository _repository;

    public FavoritoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public FavoritoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private FavoritoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.FavoritoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.FavoritoRepository;
    }

    public Task<Favorito> AddFavoritoDefaultAsync(Favorito entity) =>
        AddDefaultAsync(entity);

    public Task DeleteFavoritoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Favorito?> ReadOIDFavoritoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Favorito>> ReadAllFavoritoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Favorito> ModifierFavoritoDefaultAsync(Favorito entity) =>
        ModifierDefaultAsync(entity);

    public async Task<Favorito> NewAsync(DateTime p_fechaMarcado, int p_usuario_oid, int p_activoDigital_oid)
    {
        Usuario usuarioEntity = await _unitOfWork.UsuarioRepository.ReadAsync(p_usuario_oid)
            ?? throw new ModelException("Usuario not found for Favorito.Usuario.");

        ActivoDigital activoDigitalEntity = await _unitOfWork.ActivoDigitalRepository.ReadAsync(p_activoDigital_oid)
            ?? throw new ModelException("ActivoDigital not found for Favorito.ActivoDigital.");

        Favorito entity = new Favorito
        {
            FechaMarcado = p_fechaMarcado,
            Usuario = usuarioEntity,
            ActivoDigital = activoDigitalEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Favorito entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Favorito not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, DateTime p_fechaMarcado)
    {
        Favorito entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Favorito not found.");
        entity.FechaMarcado = p_fechaMarcado;
        await _repository.SaveAsync(entity);
    }

    public async Task<Favorito?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Favorito>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
