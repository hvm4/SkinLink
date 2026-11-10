// HARIA-WEB GENERATED sha256:a2b31f1d3b5ff4e79fadf7b69e8b7e1931cc9e8060f65d486d69461b0b899be1
using System.ComponentModel.DataAnnotations;

namespace Skinlink.Web.ViewModels;

public sealed class LoginViewModel
{
    public string UserClass { get; set; } = "Usuario";
    public string? Identifier { get; set; }
    [Display(Name = "id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Debes indicar {0}.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
