using System.ComponentModel.DataAnnotations;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class CategoryForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El color es obligatorio.")]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Usa un color hexadecimal, por ejemplo #0D4D84.")]
    [Display(Name = "Color")]
    public string Color { get; set; } = Entities.Category.DefaultColor;

    [Display(Name = "Activa")]
    public bool Available { get; set; } = true;
}
