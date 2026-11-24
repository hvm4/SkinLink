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
}

public static partial class HariaWebAuthentication
{
    // Punto de extensión de HARIA Web: se ejecuta en el login, después de validar
    // las credenciales y cargar la entidad desde la base de datos (serverEntity).
    // El rol se decide en el servidor según la clase real de la cuenta
    // (Administrador hereda de Usuario), nunca a partir de datos del formulario.
    static partial void AddTrustedClaims(List<Claim> claims, string domainClass, object identifier, object? serverEntity)
    {
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
