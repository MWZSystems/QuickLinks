using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class SettingsForm
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre de la aplicación")]
    public string AppName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre de la empresa (pie de página)")]
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "El color es obligatorio.")]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Usa un color hexadecimal, por ejemplo #0D4D84.")]
    [Display(Name = "Color principal")]
    public string PrimaryColor { get; set; } = string.Empty;

    [Required(ErrorMessage = "La expresión es obligatoria.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Expresión para validar enlaces")]
    public string LinkValidationRegex { get; set; } = string.Empty;

    [Required(ErrorMessage = "La zona horaria es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Zona horaria")]
    public string TimeZone { get; set; } = string.Empty;

    [Display(Name = "Logo del cliente")]
    public IFormFile? Logo { get; set; }

    public bool RemoveLogo { get; set; }

    [ValidateNever]
    public string? CurrentLogoUrl { get; set; }
}
