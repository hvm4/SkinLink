using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class MonederoViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(MonederoViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Display(Name = "saldoDisponible / valor")]
    public double SaldoDisponibleValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "saldoDisponible / divisa")]
    public string SaldoDisponibleDivisa { get; set; } = string.Empty;

    [Display(Name = "saldoPendiente / valor")]
    public double SaldoPendienteValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "saldoPendiente / divisa")]
    public string SaldoPendienteDivisa { get; set; } = string.Empty;

    [UIHint("HariaAssociation")]
    [Display(Name = "Usuario")]
    public int? UsuarioId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> UsuarioOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Usuario")]
    public string? UsuarioLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(MonederoViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
