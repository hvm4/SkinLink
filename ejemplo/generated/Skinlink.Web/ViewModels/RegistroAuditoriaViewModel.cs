using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class RegistroAuditoriaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(RegistroAuditoriaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [UIHint("HariaReadOnly:Edit")]
    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Accion { get; set; } = string.Empty;

    [UIHint("HariaReadOnly:Edit")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Administrador")]
    public int? AdministradorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AdministradorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Administrador")]
    public string? AdministradorLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(RegistroAuditoriaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
