using System.ComponentModel.DataAnnotations;

namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Campos de vigencia comunes a avisos y mantenimientos, capturados en la hora local configurada.</summary>
public abstract class ScheduleForm : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Inicio")]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Fin (opcional)")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Activo")]
    public bool Available { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate is not null && EndDate is not null && EndDate <= StartDate)
        {
            yield return new ValidationResult("El fin debe ser posterior al inicio.", [nameof(EndDate)]);
        }
    }
}
