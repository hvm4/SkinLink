global using Skinlink.Web.Utils;

// Fichero manual (no lo genera HARIA Web). Estándar común de mensajes al usuario para toda la web.
//
// Convención (ver README, sección "Mensajes al usuario"):
//   - Resultado de una operación:  TempData["Exito"] = "..."  /  TempData["Error"] = this.MensajeDeError(ex);
//   - Errores de formulario:       ModelState.AddModelError(string.Empty, this.MensajeDeError(ex));
//   - En la vista:                 <partial name="_MensajesOperacion" />
using ejemplo.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Skinlink.Web.Utils;

/// <summary>
/// Traduce una excepción al texto que ve el usuario.
/// Los errores de negocio (<see cref="ModelException"/>) se muestran tal cual; los casos de uso los
/// envuelven en "Application error while executing use case ...", así que se usa el más interno.
/// Cualquier otro error es técnico: se registra en el log del servidor y el usuario recibe un mensaje genérico.
/// </summary>
public static class MensajesUsuario
{
    public const string ErrorGenerico = "No se ha podido completar la operación. Inténtalo de nuevo más tarde.";

    /// <summary>Indica si la excepción (o alguna interna) es un error de negocio con mensaje para el usuario.</summary>
    public static bool EsDeNegocio(Exception ex) => MensajeDeNegocio(ex) is not null;

    /// <summary>Texto para el usuario, sin registrar nada en el log.</summary>
    public static string Texto(Exception ex) => MensajeDeNegocio(ex) ?? ErrorGenerico;

    /// <summary>
    /// Texto para el usuario desde un controlador. Si el error es técnico, además lo registra en el log
    /// del servidor para no perder el detalle que ya no se muestra en pantalla.
    /// </summary>
    public static string MensajeDeError(this ControllerBase controller, Exception ex)
    {
        var negocio = MensajeDeNegocio(ex);
        if (negocio is not null) return negocio;

        var logger = controller.HttpContext?.RequestServices?.GetService<ILoggerFactory>()?.CreateLogger(controller.GetType());
        logger?.LogError(ex, "Error técnico en {Controlador}.{Accion}.",
            controller.ControllerContext?.ActionDescriptor?.ControllerName,
            controller.ControllerContext?.ActionDescriptor?.ActionName);
        return ErrorGenerico;
    }

    // Se mira la causa original (la excepción más interna): si es de negocio, su mensaje es para el usuario.
    // Si es técnica (base de datos, nulos...), aunque el caso de uso la envuelva en una ModelException
    // ("Application error while executing use case ..."), no se enseña.
    private static string? MensajeDeNegocio(Exception ex)
    {
        var causa = ex;
        while (causa.InnerException is not null) causa = causa.InnerException;
        return causa is ModelException && !string.IsNullOrWhiteSpace(causa.Message) ? causa.Message : null;
    }
}
