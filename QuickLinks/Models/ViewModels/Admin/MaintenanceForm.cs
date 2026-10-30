using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class MaintenanceForm : ScheduleForm
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una aplicación.")]
    [Display(Name = "Aplicación")]
    public int ApplicationId { get; set; }

    [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Mensaje (opcional)")]
    public string? CustomMessage { get; set; }

    [Display(Name = "Bloquear la aplicación en el portal mientras esté vigente")]
    public bool DisableApplication { get; set; } = true;

    [ValidateNever]
    public IReadOnlyList<SelectListItem> Applications { get; set; } = [];
}
