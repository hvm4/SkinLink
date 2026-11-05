using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class AdministradorService : ServiceEntity<Administrador>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdministradorRepository _repository;
    private readonly IHariaPasswordHasher _passwordHasher;

    public AdministradorService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public AdministradorService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private AdministradorService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.AdministradorRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.AdministradorRepository;
        _passwordHasher = unitOfWork.PasswordHasher;
    }

    public Task<Administrador> AddAdministradorDefaultAsync(Administrador entity)
    {
        HashPasswordProperties(entity);
        return AddDefaultAsync(entity);
    }

    public Task DeleteAdministradorDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Administrador?> ReadOIDAdministradorDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Administrador>> ReadAllAdministradorDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Administrador> ModifierAdministradorDefaultAsync(Administrador entity)
    {
        HashPasswordProperties(entity);
        return ModifierDefaultAsync(entity);
    }

    private void HashPasswordProperties(Administrador entity)
    {
        if (entity.Contrasena is not null)
        {
            entity.Contrasena = _passwordHasher.HashPasswordIfNeeded(entity.Contrasena);
        }
    }

    public async Task<Administrador> NewAsync(string p_nombreUsuario, string p_correo, string p_contrasena, DateTime p_fechaNacimiento, EstadoCuenta p_estado, int p_nivel, int p_experiencia, DateTime p_fechaUltimoLogin, bool p_temaModoOscuro, string p_idioma, Monedero p_monedero, InventarioPrivado p_inventario, string p_codigoAuditor)
    {
        Administrador entity = new Administrador
        {
            NombreUsuario = p_nombreUsuario,
            Correo = p_correo,
            Contrasena = _passwordHasher.HashPasswordIfNeeded(p_contrasena),
            FechaNacimiento = p_fechaNacimiento,
            Estado = p_estado,
            Nivel = p_nivel,
            Experiencia = p_experiencia,
            FechaUltimoLogin = p_fechaUltimoLogin,
            TemaModoOscuro = p_temaModoOscuro,
            Idioma = p_idioma,
            CodigoAuditor = p_codigoAuditor,
            Monedero = p_monedero,
            Inventario = p_inventario,
        };

        entity.Monedero.Usuario = entity;

        entity.Inventario.Usuario = entity;

        await _repository.SaveAsync(entity);
        return entity;
    }

    public async Task DestroyAsync(int p_oid)
    {
        Administrador entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Administrador not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombreUsuario, string p_correo, string p_contrasena, DateTime p_fechaNacimiento, EstadoCuenta p_estado, int p_nivel, int p_experiencia, DateTime p_fechaUltimoLogin, bool p_temaModoOscuro, string p_idioma, Monedero p_monedero, InventarioPrivado p_inventario, string p_codigoAuditor)
    {
        Administrador entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Administrador not found.");
        entity.NombreUsuario = p_nombreUsuario;
        entity.Correo = p_correo;
        entity.Contrasena = _passwordHasher.HashPasswordIfNeeded(p_contrasena);
        entity.FechaNacimiento = p_fechaNacimiento;
        entity.Estado = p_estado;
        entity.Nivel = p_nivel;
        entity.Experiencia = p_experiencia;
        entity.FechaUltimoLogin = p_fechaUltimoLogin;
        entity.TemaModoOscuro = p_temaModoOscuro;
        entity.Idioma = p_idioma;
        entity.CodigoAuditor = p_codigoAuditor;
        await _repository.SaveAsync(entity);
    }

    public async Task<Administrador?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Administrador>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

}
