using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class CajaMisteriosaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(CajaMisteriosaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "precioApertura / valor")]
    public double PrecioAperturaValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "precioApertura / divisa")]
    public string PrecioAperturaDivisa { get; set; } = string.Empty;

    public bool Activa { get; set; }

    // HARIA-WEB COMPOSITION {"property":"Probabilidades","fullName":"Skinlink.Web.ViewModels.ProbabilidadDropViewModel","fields":[{"name":"Id","type":"int","key":true},{"name":"PorcentajeDrop","type":"double","key":false},{"name":"ActivoRecompensaId","type":"int?","association":true},{"name":"ActivoRecompensaLabel","type":"string?","readOnly":true}]}
    [UIHint("HariaComposition")]
    [MinLength(1)]
    public List<ProbabilidadDropViewModel> Probabilidades { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public ProbabilidadDropViewModel ProbabilidadesTemplate { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(CajaMisteriosaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
