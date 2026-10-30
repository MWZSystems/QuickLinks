using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class UserForm
{
    public const int MinPasswordLength = 12;

    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre completo")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9._@\-]+$", ErrorMessage = "Usa solo letras, números, punto, guion, guion bajo o @.")]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un rol.")]
    [Display(Name = "Rol")]
    public int RoleId { get; set; } = Entities.Role.AdministratorId;

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = MinPasswordLength, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Activo")]
    public bool Available { get; set; } = true;

    [ValidateNever]
    public IReadOnlyList<SelectListItem> Roles { get; set; } = [];
}
