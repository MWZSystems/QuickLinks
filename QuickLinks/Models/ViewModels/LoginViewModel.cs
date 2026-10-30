using System.ComponentModel.DataAnnotations;

namespace QuickLinks.Models.ViewModels;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Escribe tu usuario.")]
    [StringLength(50)]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
