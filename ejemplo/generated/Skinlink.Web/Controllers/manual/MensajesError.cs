// Fichero manual (no lo genera HARIA Web) — Miembro 4.
using ejemplo.Application.Exceptions;

namespace Skinlink.Web.Controllers;

/// <summary>
/// Los casos de uso envuelven el error de negocio en "Application error while executing use case ...".
/// Esto devuelve el mensaje de la ModelException más interna, que es el que entiende el usuario.
/// </summary>
internal static class MensajesError
{
    public static bool EsDeNegocio(Exception ex)
    {
        for (var actual = ex; actual is not null; actual = actual.InnerException)
        {
            if (actual is ModelException) return true;
        }
        return false;
    }

    public static string Mensaje(Exception ex)
    {
        string? mensaje = null;
        for (var actual = ex; actual is not null; actual = actual.InnerException)
        {
            if (actual is ModelException) mensaje = actual.Message;
        }
        return mensaje ?? "No se ha podido completar la operación.";
    }
}
