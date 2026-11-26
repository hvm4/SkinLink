using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class ResenaVendedorViewModel : IValidatableObject
{
    /*PROTECTED REGION ID(ResenaVendedorViewModel_generated) START*/
    [Key]
    public int Id { get; set; }

    public int Puntuacion { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    public string Comentario { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    public bool Oculta { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Comprador")]
    public int? CompradorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> CompradorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Comprador")]
    public string? CompradorLabel { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [UIHint("HariaAssociation")]
    [Display(Name = "Vendedor")]
    public int? VendedorId { get; set; }
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> VendedorOptions { get; set; } = new();
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever, UIHint("HariaAssociationLabel"), Display(Name = "Vendedor")]
    public string? VendedorLabel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in ValidateAdditional(validationContext)) yield return result;
    }
    /*PROTECTED REGION END*/

    /*PROTECTED REGION ID(ResenaVendedorViewModel_manual) ENABLED START*/
    // Miembro 4: la valoración de vendedores es una nota numérica de 1 a 5 (servidor y cliente vía ModelState).
    public const int PuntuacionMinima = 1;
    public const int PuntuacionMaxima = 5;
    public const int LongitudMaximaComentario = 500;

    private IEnumerable<ValidationResult> ValidateAdditional(ValidationContext validationContext)
    {
        if (Puntuacion < PuntuacionMinima || Puntuacion > PuntuacionMaxima)
            yield return new ValidationResult(
                $"La puntuación debe estar entre {PuntuacionMinima} y {PuntuacionMaxima}.", new[] { nameof(Puntuacion) });

        if (!string.IsNullOrEmpty(Comentario) && Comentario.Length > LongitudMaximaComentario)
            yield return new ValidationResult(
                $"El comentario no puede superar los {LongitudMaximaComentario} caracteres.", new[] { nameof(Comentario) });

        if (CompradorId is not null && CompradorId == VendedorId)
            yield return new ValidationResult(
                "Un usuario no puede valorarse a sí mismo.", new[] { nameof(VendedorId) });
    }

    /*PROTECTED REGION END*/
}
