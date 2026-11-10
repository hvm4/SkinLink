using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class ActivoDigitalViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(ActivoDigitalViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Coleccion { get; set; } = string.Empty;

    [Display(Name = "precioEstimado / valor")]
    public double PrecioEstimadoValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "precioEstimado / divisa")]
    public string PrecioEstimadoDivisa { get; set; } = string.Empty;

    [EnumDataType(typeof(ejemplo.Domain.Enums.Rareza), ErrorMessage = "El valor de {0} no es válido.")]
    [Display(Name = "atributos / rareza")]
    public ejemplo.Domain.Enums.Rareza AtributosRareza { get; set; }

    [EnumDataType(typeof(ejemplo.Domain.Enums.Condicion), ErrorMessage = "El valor de {0} no es válido.")]
    [Display(Name = "atributos / condicion")]
    public ejemplo.Domain.Enums.Condicion AtributosCondicion { get; set; }

    [Display(Name = "atributos / floatVal")]
    public double AtributosFloatVal { get; set; }

    [Display(Name = "atributos / patron")]
    public int AtributosPatron { get; set; }

    public bool EnVenta { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string IdentificadorUnico { get; set; } = string.Empty;

    [UIHint("HariaAssociation")]
    [Display(Name = "Inventario")]
    public int? InventarioId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> InventarioOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Inventario")]
    public string? InventarioLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Venta")]
    public int? VentaId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> VentaOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Venta")]
    public string? VentaLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "IntercambioOfrecido")]
    public int? IntercambioOfrecidoId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> IntercambioOfrecidoOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "IntercambioOfrecido")]
    public string? IntercambioOfrecidoLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "IntercambioSolicitado")]
    public int? IntercambioSolicitadoId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> IntercambioSolicitadoOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "IntercambioSolicitado")]
    public string? IntercambioSolicitadoLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ActivoDigitalViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
