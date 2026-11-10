using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class TransaccionViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(TransaccionViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [EnumDataType(typeof(ejemplo.Domain.Enums.TipoTransaccion), ErrorMessage = "El valor de {0} no es válido.")]
    public ejemplo.Domain.Enums.TipoTransaccion Tipo { get; set; }

    [Display(Name = "monto / valor")]
    public double MontoValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "monto / divisa")]
    public string MontoDivisa { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Concepto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Monedero")]
    public int? MonederoId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> MonederoOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Monedero")]
    public string? MonederoLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "VentaCobro")]
    public int? VentaCobroId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> VentaCobroOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "VentaCobro")]
    public string? VentaCobroLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "VentaPago")]
    public int? VentaPagoId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> VentaPagoOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "VentaPago")]
    public string? VentaPagoLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(TransaccionViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
