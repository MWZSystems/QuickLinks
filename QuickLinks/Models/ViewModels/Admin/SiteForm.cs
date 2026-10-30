using System.ComponentModel.DataAnnotations;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class SiteForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Dirección")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Available { get; set; } = true;
}
