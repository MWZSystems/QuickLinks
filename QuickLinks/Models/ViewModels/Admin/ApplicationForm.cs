using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class ApplicationForm
{
    public const int MaxCarouselImages = 10;

    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El enlace es obligatorio.")]
    [StringLength(500, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Enlace (URL)")]
    public string Link { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Versión")]
    public string? Version { get; set; }

    [StringLength(200, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Responsable (SME)")]
    public string? Sme { get; set; }

    [StringLength(200, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Manual de usuario")]
    public string? UserManual { get; set; }

    [Display(Name = "Activa")]
    public bool Available { get; set; } = true;

    [Display(Name = "Logo")]
    public IFormFile? Logo { get; set; }

    public bool RemoveLogo { get; set; }

    [Display(Name = "Agregar imágenes al carrusel")]
    public List<IFormFile> CarouselFiles { get; set; } = [];

    public List<int> RemoveCarouselIds { get; set; } = [];

    public List<int> CategoryIds { get; set; } = [];

    public List<int> BuildingIds { get; set; } = [];

    public List<int> GroupIds { get; set; } = [];

    [ValidateNever]
    public string? CurrentLogoUrl { get; set; }

    [ValidateNever]
    public IReadOnlyList<(int Id, string Url)> CurrentCarousel { get; set; } = [];

    [ValidateNever]
    public ApplicationFormOptions Options { get; set; } = new();
}
