using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class InventarioPrivadoViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(InventarioPrivadoViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Display(Name = "valorEstimadoTotal / valor")]
    public double ValorEstimadoTotalValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "valorEstimadoTotal / divisa")]
    public string ValorEstimadoTotalDivisa { get; set; } = string.Empty;

    public bool EsPublico { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Usuario")]
    public int? UsuarioId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> UsuarioOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Usuario")]
    public string? UsuarioLabel { get; set; }

    // HARIA-WEB COMPOSITION {"property":"Activos","fullName":"Skinlink.Web.ViewModels.ActivoDigitalViewModel","fields":[{"name":"Id","type":"int","key":true},{"name":"Nombre","type":"string","key":false},{"name":"Coleccion","type":"string","key":false},{"name":"PrecioEstimadoValor","type":"double","key":false},{"name":"PrecioEstimadoDivisa","type":"string","key":false},{"name":"AtributosRareza","type":"ejemplo.Domain.Enums.Rareza","key":false,"enumType":"ejemplo.Domain.Enums.Rareza"},{"name":"AtributosCondicion","type":"ejemplo.Domain.Enums.Condicion","key":false,"enumType":"ejemplo.Domain.Enums.Condicion"},{"name":"AtributosFloatVal","type":"double","key":false},{"name":"AtributosPatron","type":"int","key":false},{"name":"EnVenta","type":"bool","key":false},{"name":"IdentificadorUnico","type":"string","key":false},{"name":"VentaId","type":"int?","association":true},{"name":"VentaLabel","type":"string?","readOnly":true},{"name":"IntercambioOfrecidoId","type":"int?","association":true},{"name":"IntercambioOfrecidoLabel","type":"string?","readOnly":true},{"name":"IntercambioSolicitadoId","type":"int?","association":true},{"name":"IntercambioSolicitadoLabel","type":"string?","readOnly":true}]}
    [UIHint("HariaComposition")]
    [MinLength(0)]
    public List<ActivoDigitalViewModel> Activos { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public ActivoDigitalViewModel ActivosTemplate { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(InventarioPrivadoViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
