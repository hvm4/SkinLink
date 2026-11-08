using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class ConfiguracionSistemaService : ServiceEntity<ConfiguracionSistema>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguracionSistemaRepository _repository;

    public ConfiguracionSistemaService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public ConfiguracionSistemaService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private ConfiguracionSistemaService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.ConfiguracionSistemaRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.ConfiguracionSistemaRepository;
    }

    public Task<ConfiguracionSistema> AddConfiguracionSistemaDefaultAsync(ConfiguracionSistema entity) =>
        AddDefaultAsync(entity);

    public Task DeleteConfiguracionSistemaDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<ConfiguracionSistema?> ReadOIDConfiguracionSistemaDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<ConfiguracionSistema>> ReadAllConfiguracionSistemaDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<ConfiguracionSistema> ModifierConfiguracionSistemaDefaultAsync(ConfiguracionSistema entity) =>
        ModifierDefaultAsync(entity);

    public async Task<ConfiguracionSistema> NewAsync(double p_limitePrecioBajo, double p_limitePrecioMedio, double p_comisionPrecioBajo, double p_comisionPrecioMedio, double p_comisionPrecioAlto, double p_retiroMinimo, double p_retiroMaximoDiario)
    {
        ConfiguracionSistema entity = new ConfiguracionSistema
        {
            LimitePrecioBajo = p_limitePrecioBajo,
            LimitePrecioMedio = p_limitePrecioMedio,
            ComisionPrecioBajo = p_comisionPrecioBajo,
            ComisionPrecioMedio = p_comisionPrecioMedio,
            ComisionPrecioAlto = p_comisionPrecioAlto,
            RetiroMinimo = p_retiroMinimo,
            RetiroMaximoDiario = p_retiroMaximoDiario,
        };

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        ConfiguracionSistema entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ConfiguracionSistema not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, double p_limitePrecioBajo, double p_limitePrecioMedio, double p_comisionPrecioBajo, double p_comisionPrecioMedio, double p_comisionPrecioAlto, double p_retiroMinimo, double p_retiroMaximoDiario)
    {
        ConfiguracionSistema entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("ConfiguracionSistema not found.");
        entity.LimitePrecioBajo = p_limitePrecioBajo;
        entity.LimitePrecioMedio = p_limitePrecioMedio;
        entity.ComisionPrecioBajo = p_comisionPrecioBajo;
        entity.ComisionPrecioMedio = p_comisionPrecioMedio;
        entity.ComisionPrecioAlto = p_comisionPrecioAlto;
        entity.RetiroMinimo = p_retiroMinimo;
        entity.RetiroMaximoDiario = p_retiroMaximoDiario;
        await _repository.SaveAsync(entity);
    }

    public async Task<ConfiguracionSistema?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<ConfiguracionSistema>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
