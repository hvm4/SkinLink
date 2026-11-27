// Fichero manual (no lo genera HARIA Web).
// Las vistas generadas por HARIA Web (listados, detalles) enseñan siempre los botones Crear, Editar y
// Eliminar, y el enlace "Volver al listado". Los controladores ya rechazan esas acciones a quien no es
// administrador; este tag helper, además, no pinta esos enlaces para que el usuario no vea botones que
// le llevan a "Acceso denegado". Se registra en Views/_ViewImports.cshtml y no hace falta tocar las vistas.
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Skinlink.Web.Utils.Security;

[HtmlTargetElement("a", Attributes = "asp-action")]
public sealed class AccionesSegunRolTagHelper : TagHelper
{
    /// <summary>Acciones del CRUD genérico que solo puede usar el administrador en cualquier entidad.</summary>
    private static readonly HashSet<string> AccionesSoloAdministrador = new(StringComparer.OrdinalIgnoreCase) { "Edit", "Delete" };

    /// <summary>Entidades en las que un usuario sí puede usar el alta genérica.</summary>
    private static readonly HashSet<string> AltaPermitidaAUsuarios = new(StringComparer.OrdinalIgnoreCase) { "ResenaVendedor" };

    /// <summary>Entidades cuyo listado completo es solo del administrador.</summary>
    private static readonly HashSet<string> ListadosSoloAdministrador = new(StringComparer.OrdinalIgnoreCase)
    {
        "Monedero", "Transaccion", "InventarioPrivado", "Usuario", "Administrador",
        "RegistroAuditoria", "ProbabilidadDrop", "ConfiguracionSistema",
    };

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    [HtmlAttributeName("asp-action")]
    public string? Accion { get; set; }

    [HtmlAttributeName("asp-controller")]
    public string? Controlador { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext.HttpContext.User.EsAdministrador()) return;

        var accion = Accion ?? string.Empty;
        var controlador = string.IsNullOrEmpty(Controlador)
            ? ViewContext.RouteData.Values["controller"]?.ToString() ?? string.Empty
            : Controlador;

        var ocultar = AccionesSoloAdministrador.Contains(accion)
            || (accion.Equals("Create", StringComparison.OrdinalIgnoreCase) && !AltaPermitidaAUsuarios.Contains(controlador))
            || (accion.Equals("Index", StringComparison.OrdinalIgnoreCase) && ListadosSoloAdministrador.Contains(controlador));

        if (ocultar) output.SuppressOutput();
    }
}
