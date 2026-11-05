using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class RegistroAuditoriaService : ServiceEntity<RegistroAuditoria>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRegistroAuditoriaRepository _repository;

    public RegistroAuditoriaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public RegistroAuditoriaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private RegistroAuditoriaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.RegistroAuditoriaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.RegistroAuditoriaRepository;
    }

    public Task<RegistroAuditoria> AddRegistroAuditoriaDefaultAsync(RegistroAuditoria entity) =>
        AddDefaultAsync(entity);

    public Task DeleteRegistroAuditoriaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<RegistroAuditoria?> ReadOIDRegistroAuditoriaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<RegistroAuditoria>> ReadAllRegistroAuditoriaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<RegistroAuditoria> ModifierRegistroAuditoriaDefaultAsync(RegistroAuditoria entity) =>
        ModifierDefaultAsync(entity);

    public async Task<RegistroAuditoria> NewAsync(string p_accion, DateTime p_fecha, int p_administrador_oid)
    {
        Administrador administradorEntity = await _unitOfWork.AdministradorRepository.ReadAsync(p_administrador_oid)
            ?? throw new ModelException("Administrador not found for RegistroAuditoria.Administrador.");

        RegistroAuditoria entity = new RegistroAuditoria
        {
            Accion = p_accion,
            Fecha = p_fecha,
            Administrador = administradorEntity,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        RegistroAuditoria entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("RegistroAuditoria not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task<RegistroAuditoria?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<RegistroAuditoria>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
