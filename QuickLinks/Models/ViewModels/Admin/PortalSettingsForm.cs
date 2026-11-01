using System.ComponentModel.DataAnnotations;
using L = QuickLinks.Services.SettingLimits;

namespace QuickLinks.Models.ViewModels.Admin;

public sealed class PortalSettingsForm
{
    [Required(ErrorMessage = "El texto es obligatorio.")]
    [StringLength(60, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Texto del buscador")]
    public string SearchPlaceholder { get; set; } = string.Empty;

    [Display(Name = "Mostrar el botón de filtros")]
    public bool ShowFilters { get; set; }

    [Display(Name = "Mostrar el contador de aplicaciones")]
    public bool ShowAppCount { get; set; }

    [Display(Name = "Mostrar fijadas y recientes")]
    public bool ShowQuickAccess { get; set; }

    [Display(Name = "Abrir las aplicaciones en una pestaña nueva")]
    public bool OpenInNewTab { get; set; }

    [Range(L.RecentMaxMin, L.RecentMaxMax, ErrorMessage = "Entre {1} y {2}.")]
    [Display(Name = "Aplicaciones recientes que se muestran")]
    public int RecentMax { get; set; }

    [Range(L.CookieDaysMin, L.CookieDaysMax, ErrorMessage = "Entre {1} y {2} días.")]
    [Display(Name = "Días que se recuerdan las recientes")]
    public int RecentDays { get; set; }

    [Range(L.CookieDaysMin, L.CookieDaysMax, ErrorMessage = "Entre {1} y {2} días.")]
    [Display(Name = "Días que se recuerdan las fijadas")]
    public int PinnedDays { get; set; }

    [Range(L.SearchDelayMin, L.SearchDelayMax, ErrorMessage = "Entre {1} y {2} ms.")]
    [Display(Name = "Espera de la búsqueda (ms)")]
    public int SearchDelayMs { get; set; }
}
