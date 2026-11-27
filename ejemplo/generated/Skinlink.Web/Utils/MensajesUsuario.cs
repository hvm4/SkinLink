global using Skinlink.Web.Utils;

using ejemplo.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Skinlink.Web.Utils;

public static class MensajesUsuario
{
    public const string ErrorGenerico = "No se ha podido completar la operación. Inténtalo de nuevo más tarde.";

    public static bool EsDeNegocio(Exception ex) => MensajeDeNegocio(ex) is not null;

    public static string Texto(Exception ex) => MensajeDeNegocio(ex) ?? ErrorGenerico;

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

    public static string MensajeDeErrorAlEliminar(this ControllerBase controller, Exception ex)
    {
        var texto = controller.MensajeDeError(ex);
        if (MensajeDeNegocio(ex) is not null) return texto;
        for (var causa = ex; causa is not null; causa = causa.InnerException)
        {
            if (causa.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
                return "No se puede eliminar este registro porque otros datos dependen de él (ventas, transacciones, intercambios, reseñas...). Elimina o modifica antes esos datos.";
        }
        return texto;
    }

    private static string? MensajeDeNegocio(Exception ex)
    {
        var causa = ex;
        while (causa.InnerException is not null) causa = causa.InnerException;
        return causa is ModelException && !string.IsNullOrWhiteSpace(causa.Message) ? causa.Message : null;
    }
}
