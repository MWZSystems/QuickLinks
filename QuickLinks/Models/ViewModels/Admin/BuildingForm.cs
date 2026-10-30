using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class BuildingForm
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un sitio.")]
    [Display(Name = "Sitio")]
    public int SiteId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Available { get; set; } = true;

    [ValidateNever]
    public IReadOnlyList<SelectListItem> Sites { get; set; } = [];
}
