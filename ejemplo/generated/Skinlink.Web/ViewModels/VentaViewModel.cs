using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class VentaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(VentaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Display(Name = "precioVenta / valor")]
    public double PrecioVentaValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "precioVenta / divisa")]
    public string PrecioVentaDivisa { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime FechaPublicacion { get; set; }

    [EnumDataType(typeof(ejemplo.Domain.Enums.EstadoVenta), ErrorMessage = "El valor de {0} no es válido.")]
    public ejemplo.Domain.Enums.EstadoVenta EstadoVenta { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Vendedor")]
    public int? VendedorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> VendedorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Vendedor")]
    public string? VendedorLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "Comprador")]
    public int? CompradorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> CompradorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Comprador")]
    public string? CompradorLabel { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "ActivoDigital")]
    public int? ActivoDigitalId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> ActivoDigitalOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "ActivoDigital")]
    public string? ActivoDigitalLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "TransaccionCobro")]
    public int? TransaccionCobroId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TransaccionCobroOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "TransaccionCobro")]
    public string? TransaccionCobroLabel { get; set; }

    [UIHint("HariaAssociation")]
    [Display(Name = "TransaccionPago")]
    public int? TransaccionPagoId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TransaccionPagoOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "TransaccionPago")]
    public string? TransaccionPagoLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(VentaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
