using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class IntercambioViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(IntercambioViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [EnumDataType(typeof(ejemplo.Domain.Enums.EstadoIntercambio), ErrorMessage = "El valor de {0} no es válido.")]
    public ejemplo.Domain.Enums.EstadoIntercambio Estado { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaCreacion { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Proponente")]
    public int? ProponenteId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> ProponenteOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Proponente")]
    public string? ProponenteLabel { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Receptor")]
    public int? ReceptorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> ReceptorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Receptor")]
    public string? ReceptorLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(IntercambioViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
