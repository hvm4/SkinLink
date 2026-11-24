// Archivo MANUAL (no generado por HARIA Web): no se sobrescribe al regenerar.
// Roles de la aplicación y utilidades para leer la identidad del usuario autenticado.
using System.Globalization;
using System.Security.Claims;

namespace Skinlink.Web.Utils.Security;

/// <summary>Nombres de los roles usados en [Authorize(Roles = ...)].</summary>
public static class Roles
{
    public const string Usuario = "Usuario";
    public const string Administrador = "Administrador";

    /// <summary>Cualquier cuenta activa (usuario normal o administrador). Las cuentas suspendidas no tienen rol.</summary>
    public const string Todos = Usuario + "," + Administrador;
}

public static partial class HariaWebAuthentication
{
    // Punto de extensión de HARIA Web: se ejecuta en el login, después de validar
    // las credenciales y cargar la entidad desde la base de datos (serverEntity).
    // El rol se decide en el servidor según la clase real de la cuenta
    // (Administrador hereda de Usuario), nunca a partir de datos del formulario.
    static partial void AddTrustedClaims(List<Claim> claims, string domainClass, object identifier, object? serverEntity)
    {
        if (serverEntity is ejemplo.Domain.Entities.Usuario cuenta)
        {
            claims.Add(new Claim(UsuarioClaimsExtensions.NombreUsuarioClaim, cuenta.NombreUsuario ?? string.Empty));
            claims.Add(new Claim(UsuarioClaimsExtensions.EstadoCuentaClaim, cuenta.Estado.ToString()));

            // Una cuenta suspendida inicia sesión pero sin ningún rol: solo ve la portada y puede cerrar sesión.
            if (cuenta.Estado != ejemplo.Domain.Enums.EstadoCuenta.Activo) return;
        }

        if (serverEntity is ejemplo.Domain.Entities.Administrador)
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.Administrador));
        }
        else if (serverEntity is ejemplo.Domain.Entities.Usuario)
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.Usuario));
        }
    }
}

public static class UsuarioClaimsExtensions
{
    public const string NombreUsuarioClaim = "SkinlinkNombreUsuario";
    public const string EstadoCuentaClaim = "SkinlinkEstadoCuenta";

    /// <summary>Nombre de usuario de la cuenta autenticada (si no, su id).</summary>
    public static string GetNombreUsuario(this ClaimsPrincipal user) =>
        user.FindFirst(NombreUsuarioClaim)?.Value ?? user.Identity?.Name ?? string.Empty;

    /// <summary>True si la cuenta estaba suspendida al iniciar sesión.</summary>
    public static bool EstaSuspendido(this ClaimsPrincipal user) =>
        user.FindFirst(EstadoCuentaClaim)?.Value == ejemplo.Domain.Enums.EstadoCuenta.Suspendido.ToString();

    /// <summary>Un administrador puede ver cualquier usuario; un usuario normal, solo a sí mismo.</summary>
    public static bool PuedeAccederAUsuario(this ClaimsPrincipal user, int usuarioId) =>
        user.EsAdministrador() || user.GetUsuarioId() == usuarioId;

    /// <summary>True si la cuenta autenticada tiene el rol Administrador.</summary>
    public static bool EsAdministrador(this ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true && user.IsInRole(Roles.Administrador);

    /// <summary>
    /// Id (OID) del Usuario autenticado, o null si no hay sesión.
    /// HARIA Web guarda el id en ClaimTypes.Name y "Clase:id" en ClaimTypes.NameIdentifier.
    /// </summary>
    public static int? GetUsuarioId(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true) return null;

        var name = user.FindFirst(ClaimTypes.Name)?.Value;
        if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)) return id;

        var nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var separator = nameIdentifier?.LastIndexOf(':') ?? -1;
        if (separator >= 0 && int.TryParse(nameIdentifier![(separator + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out id)) return id;

        return null;
    }
}
