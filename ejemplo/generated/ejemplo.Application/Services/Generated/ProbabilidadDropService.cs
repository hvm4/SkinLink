using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class ProbabilidadDropService : ServiceEntity<ProbabilidadDrop>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProbabilidadDropRepository _repository;

    public ProbabilidadDropService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public ProbabilidadDropService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private ProbabilidadDropService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.ProbabilidadDropRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.ProbabilidadDropRepository;
    }

    public Task<ProbabilidadDrop> AddProbabilidadDropDefaultAsync(ProbabilidadDrop entity) =>
        AddDefaultAsync(entity);

    public Task DeleteProbabilidadDropDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<ProbabilidadDrop?> ReadOIDProbabilidadDropDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<ProbabilidadDrop>> ReadAllProbabilidadDropDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<ProbabilidadDrop> ModifierProbabilidadDropDefaultAsync(ProbabilidadDrop entity) =>
        ModifierDefaultAsync(entity);

    public async Task<ProbabilidadDrop> NewAsync(double p_porcentajeDrop, int p_caja_oid, int p_activoRecompensa_oid)
    {
        CajaMisteriosa cajaEntity = await _unitOfWork.CajaMisteriosaRepository.ReadAsync(p_caja_oid)
            ?? throw new ModelException("CajaMisteriosa not found for ProbabilidadDrop.Caja.");

        ActivoDigital activoRecompensaEntity = await _unitOfWork.ActivoDigitalRepository.ReadAsync(p_activoRecompensa_oid)
            ?? throw new ModelException("ActivoDigital not found for ProbabilidadDrop.ActivoRecompensa.");

        ProbabilidadDrop entity = new ProbabilidadDrop
        {
            PorcentajeDrop = p_porcentajeDrop,
            Caja = cajaEntity,
            ActivoRecompensa = activoRecompensaEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        ProbabilidadDrop entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ProbabilidadDrop not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, double p_porcentajeDrop, int p_activoRecompensa_oid)
    {
        ProbabilidadDrop entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ProbabilidadDrop not found.");
        entity.PorcentajeDrop = p_porcentajeDrop;
        await _repository.SaveAsync(entity);
    }

    public async Task<ProbabilidadDrop?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<ProbabilidadDrop>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
