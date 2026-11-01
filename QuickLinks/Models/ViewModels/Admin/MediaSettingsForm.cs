using System.ComponentModel.DataAnnotations;
using L = QuickLinks.Services.SettingLimits;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class MediaSettingsForm
{
    [Range(L.MediaMaxKbMin, L.MediaMaxKbMax, ErrorMessage = "Entre {1} y {2} KB.")]
    [Display(Name = "Peso máximo por imagen (KB)")]
    public int MaxFileSizeKb { get; set; }

    [Range(L.CarouselMaxMin, L.CarouselMaxMax, ErrorMessage = "Entre {1} y {2} imágenes.")]
    [Display(Name = "Imágenes máximas en el carrusel")]
    public int CarouselMaxImages { get; set; }
}
