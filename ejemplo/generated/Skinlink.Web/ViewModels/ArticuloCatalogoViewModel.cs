using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class ArticuloCatalogoViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(ArticuloCatalogoViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Coleccion { get; set; } = string.Empty;

    [EnumDataType(typeof(ejemplo.Domain.Enums.Rareza), ErrorMessage = "El valor de {0} no es válido.")]
    public ejemplo.Domain.Enums.Rareza Rareza { get; set; }

    [Display(Name = "precioReferencia / valor")]
    public double PrecioReferenciaValor { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [Display(Name = "precioReferencia / divisa")]
    public string PrecioReferenciaDivisa { get; set; } = string.Empty;

    public bool Habilitado { get; set; }

    public double? PorcentajeDescuento { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaFinDescuento { get; set; }



    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ArticuloCatalogoViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
