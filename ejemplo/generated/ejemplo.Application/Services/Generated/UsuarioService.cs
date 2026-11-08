using ejemplo.Application.Abstractions;
using ejemplo.Application.Exceptions;
using ejemplo.Domain.Entities;
using ejemplo.Domain.Enums;

namespace ejemplo.Application.Services;

public sealed partial class UsuarioService : ServiceEntity<Usuario>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioRepository _repository;
    private readonly IHariaPasswordHasher _passwordHasher;
    private readonly IHariaTokenGenerator _tokenGenerator;

    public UsuarioService(IUnitOfWork unitOfWork)
        : this(unitOfWork, ownsUnitOfWork: false)
    {
    }

    public UsuarioService(IUnitOfWorkFactory unitOfWorkFactory)
        : this(unitOfWorkFactory.Create(), ownsUnitOfWork: true)
    {
    }

    private UsuarioService(IUnitOfWork unitOfWork, bool ownsUnitOfWork) : base(unitOfWork, unitOfWork.UsuarioRepository, ownsUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.UsuarioRepository;
        _passwordHasher = unitOfWork.PasswordHasher;
        _tokenGenerator = unitOfWork.TokenGenerator;
    }

    public Task<Usuario> AddUsuarioDefaultAsync(Usuario entity)
    {
        HashPasswordProperties(entity);
        return AddDefaultAsync(entity);
    }

    public Task DeleteUsuarioDefaultAsync(int id) =>
        DeleteDefaultAsync(id);

    public Task<Usuario?> ReadOIDUsuarioDefaultAsync(int id) =>
        ReadOIDDefaultAsync(id);

    public Task<IReadOnlyList<Usuario>> ReadAllUsuarioDefaultAsync() =>
        ReadAllDefaultAsync();

    public Task<Usuario> ModifierUsuarioDefaultAsync(Usuario entity)
    {
        HashPasswordProperties(entity);
        return ModifierDefaultAsync(entity);
    }

    private void HashPasswordProperties(Usuario entity)
    {
        if (entity.Contrasena is not null)
        {
            entity.Contrasena = _passwordHasher.HashPasswordIfNeeded(entity.Contrasena);
        }
    }

    public async Task<Usuario> NewAsync(string p_nombreUsuario, string p_correo, string p_contrasena, DateTime p_fechaNacimiento, EstadoCuenta p_estado, int p_nivel, int p_experiencia, DateTime p_fechaUltimoLogin, bool p_temaModoOscuro, string p_idioma, bool p_verificado, Monedero p_monedero, InventarioPrivado p_inventario)
    {
        Usuario entity = new Usuario
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
            Verificado = p_verificado,
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
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
        await _repository.DeleteAsync(entity);
    }

    public async Task ModifyAsync(int p_oid, string p_nombreUsuario, string p_correo, string p_contrasena, DateTime p_fechaNacimiento, EstadoCuenta p_estado, int p_nivel, int p_experiencia, DateTime p_fechaUltimoLogin, bool p_temaModoOscuro, string p_idioma, bool p_verificado, Monedero p_monedero, InventarioPrivado p_inventario)
    {
        Usuario entity = await _repository.ReadAsync(p_oid)
            ?? throw new ModelException("Usuario not found.");
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
        entity.Verificado = p_verificado;
        await _repository.SaveAsync(entity);
    }

    public async Task<Usuario?> ReadOIDAsync(int p_oid)
    {
        return await _repository.ReadAsync(p_oid);
    }

    public async Task<IReadOnlyList<Usuario>> ReadAllAsync()
    {
        return await _repository.ReadAllAsync();
    }

    public async Task<string?> LoginAsync(int p_oid, string p_password)
    {
        try
        {
            Usuario? entity = await _repository.ReadAsync(p_oid);
            if (entity is null)
            {
                return null;
            }

            if (!_passwordHasher.VerifyPassword(entity.Contrasena, p_password))
            {
                return null;
            }

            var tokenSubject = Convert.ToString(entity.Id, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
            var tokenClaims = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["id"] = Convert.ToString(entity.Id, System.Globalization.CultureInfo.InvariantCulture),
            };
            return _tokenGenerator.CreateToken(tokenSubject, tokenClaims);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<Usuario>> FiltrarUsuariosPorEstadoAsync(EstadoCuenta p_estado)
    {
        return await _repository.FiltrarUsuariosPorEstadoAsync(p_estado);
    }

}
