using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class UsuarioViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(UsuarioViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Correo { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    [UIHint("HariaPassword")]
    [System.Text.Json.Serialization.JsonIgnore]
    public string? Contrasena { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaNacimiento { get; set; }

    [EnumDataType(typeof(ejemplo.Domain.Enums.EstadoCuenta), ErrorMessage = "El valor de {0} no es válido.")]
    public ejemplo.Domain.Enums.EstadoCuenta Estado { get; set; }

    public int Nivel { get; set; }

    public int Experiencia { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaUltimoLogin { get; set; }

    public bool TemaModoOscuro { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Idioma { get; set; } = string.Empty;

    public bool Verificado { get; set; }

    [UIHint("HariaReadOnly:Create,Edit")]
    [DataType(DataType.Date)]
    public DateTime? FechaUltimaRecompensa { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Monedero")]
    public int? MonederoId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> MonederoOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Monedero")]
    public string? MonederoLabel { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Inventario")]
    public int? InventarioId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> InventarioOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Inventario")]
    public string? InventarioLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(UsuarioViewModel_manual) ENABLED START*/
    // Add custom properties and validation here.
    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        yield break;
    }

    /*PROTECTED REGION END*/
}
