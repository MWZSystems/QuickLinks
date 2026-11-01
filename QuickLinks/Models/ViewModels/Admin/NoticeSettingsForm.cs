using System.ComponentModel.DataAnnotations;
using L = QuickLinks.Services.SettingLimits;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class NoticeSettingsForm
{
    [Required(ErrorMessage = "La etiqueta es obligatoria.")]
    [StringLength(30, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Etiqueta de la franja")]
    public string Label { get; set; } = string.Empty;

    [Required(ErrorMessage = "El color es obligatorio.")]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Usa un color hexadecimal, por ejemplo #FCD34D.")]
    [Display(Name = "Color de la franja")]
    public string Color { get; set; } = string.Empty;

    [Range(L.NoticesSpeedMin, L.NoticesSpeedMax, ErrorMessage = "Entre {1} y {2} segundos.")]
    [Display(Name = "Duración de una vuelta (segundos)")]
    public int SpeedSeconds { get; set; }
}
