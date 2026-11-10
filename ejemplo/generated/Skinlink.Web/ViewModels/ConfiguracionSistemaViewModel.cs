using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class ConfiguracionSistemaViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(ConfiguracionSistemaViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    public double LimitePrecioBajo { get; set; }

    public double LimitePrecioMedio { get; set; }

    public double ComisionPrecioBajo { get; set; }

    public double ComisionPrecioMedio { get; set; }

    public double ComisionPrecioAlto { get; set; }

    public double RetiroMinimo { get; set; }

    public double RetiroMaximoDiario { get; set; }



    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ConfiguracionSistemaViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
