using System.Globalization;
using System.Security.Claims;

namespace Skinlink.Web.Utils.Security;

public static class Roles
{
    public const string Usuario = "Usuario";
    public const string Administrador = "Administrador";

    public const string Todos = Usuario + "," + Administrador;
}

public static partial class HariaWebAuthentication
{
    static partial void AddTrustedClaims(List<Claim> claims, string domainClass, object identifier, object? serverEntity)
    {
        if (serverEntity is ejemplo.Domain.Entities.Usuario cuenta)
        {
            // Id numérico de la cuenta, tomado de la entidad del servidor. Con el login por correo,
            // el identificador del login (ClaimTypes.Name) es el correo y no el Id.
            claims.Add(new Claim(UsuarioClaimsExtensions.UsuarioIdClaim, cuenta.Id.ToString(CultureInfo.InvariantCulture)));
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
    public const string UsuarioIdClaim = "SkinlinkUsuarioId";
    public const string NombreUsuarioClaim = "SkinlinkNombreUsuario";
    public const string EstadoCuentaClaim = "SkinlinkEstadoCuenta";

    public static string GetNombreUsuario(this ClaimsPrincipal user) =>
        user.FindFirst(NombreUsuarioClaim)?.Value ?? user.Identity?.Name ?? string.Empty;

    public static bool EstaSuspendido(this ClaimsPrincipal user) =>
        user.FindFirst(EstadoCuentaClaim)?.Value == ejemplo.Domain.Enums.EstadoCuenta.Suspendido.ToString();

    public static bool PuedeAccederAUsuario(this ClaimsPrincipal user, int usuarioId) =>
        user.EsAdministrador() || user.GetUsuarioId() == usuarioId;

    public static bool EsAdministrador(this ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true && user.IsInRole(Roles.Administrador);

    public static int? GetUsuarioId(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true) return null;

        var propio = user.FindFirst(UsuarioIdClaim)?.Value;
        if (int.TryParse(propio, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idPropio)) return idPropio;

        var name = user.FindFirst(ClaimTypes.Name)?.Value;
        if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)) return id;

        var nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var separator = nameIdentifier?.LastIndexOf(':') ?? -1;
        if (separator >= 0 && int.TryParse(nameIdentifier![(separator + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out id)) return id;

        return null;
    }
}
