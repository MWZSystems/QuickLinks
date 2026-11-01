using System.ComponentModel.DataAnnotations;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class RegionalSettingsForm
{
    [Required(ErrorMessage = "La zona horaria es obligatoria.")]
    [StringLength(100)]
    [Display(Name = "Zona horaria")]
    public string TimeZone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La expresión es obligatoria.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Expresión para validar enlaces")]
    public string LinkValidationRegex { get; set; } = string.Empty;
}
