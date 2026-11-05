using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.ValueObjects;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class ArticuloCatalogoService : ServiceEntity<ArticuloCatalogo>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IArticuloCatalogoRepository _repository;

    public ArticuloCatalogoService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public ArticuloCatalogoService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private ArticuloCatalogoService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.ArticuloCatalogoRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.ArticuloCatalogoRepository;
    }

    public Task<ArticuloCatalogo> AddArticuloCatalogoDefaultAsync(ArticuloCatalogo entity) =>
        AddDefaultAsync(entity);

    public Task DeleteArticuloCatalogoDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<ArticuloCatalogo?> ReadOIDArticuloCatalogoDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<ArticuloCatalogo>> ReadAllArticuloCatalogoDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<ArticuloCatalogo> ModifierArticuloCatalogoDefaultAsync(ArticuloCatalogo entity) =>
        ModifierDefaultAsync(entity);

    public async Task<ArticuloCatalogo> NewAsync(string p_nombre, string p_coleccion, Rareza p_rareza, Moneda p_precioReferencia, bool p_habilitado, double p_porcentajeDescuento, DateTime p_fechaFinDescuento)
    {
        ArticuloCatalogo entity = new ArticuloCatalogo
        {
            Nombre = p_nombre,
            Coleccion = p_coleccion,
            Rareza = p_rareza,
            PrecioReferencia = p_precioReferencia,
            Habilitado = p_habilitado,
            PorcentajeDescuento = p_porcentajeDescuento,
            FechaFinDescuento = p_fechaFinDescuento,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        ArticuloCatalogo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ArticuloCatalogo not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombre, string p_coleccion, Rareza p_rareza, Moneda p_precioReferencia, bool p_habilitado, double p_porcentajeDescuento, DateTime p_fechaFinDescuento)
    {
        ArticuloCatalogo entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ArticuloCatalogo not found.");
        entity.Nombre = p_nombre;
        entity.Coleccion = p_coleccion;
        entity.Rareza = p_rareza;
        entity.PrecioReferencia = p_precioReferencia;
        entity.Habilitado = p_habilitado;
        entity.PorcentajeDescuento = p_porcentajeDescuento;
        entity.FechaFinDescuento = p_fechaFinDescuento;
        await _repository.SaveAsync(entity);
    }

    public async Task<ArticuloCatalogo?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<ArticuloCatalogo>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
