using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Skinlink.Web.Utils.Security;

[HtmlTargetElement("a", Attributes = "asp-action")]
public sealed class AccionesSegunRolTagHelper : TagHelper
{
    private static readonly HashSet<string> AccionesSoloAdministrador = new(StringComparer.OrdinalIgnoreCase) { "Edit", "Delete" };

    private static readonly HashSet<string> AltaPermitidaAUsuarios = new(StringComparer.OrdinalIgnoreCase) { "ResenaVendedor" };

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
