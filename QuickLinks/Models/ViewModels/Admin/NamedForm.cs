using System.ComponentModel.DataAnnotations;

namespace QuickLinks.Models.ViewModels.Admin;

/// <summary>Formulario de catálogos que solo tienen nombre y estado (grupos).</summary>
public sealed class NamedForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Available { get; set; } = true;
}
