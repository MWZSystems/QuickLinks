using System.ComponentModel.DataAnnotations;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class AdviceForm : ScheduleForm
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contenido es obligatorio.")]
    [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;
}
