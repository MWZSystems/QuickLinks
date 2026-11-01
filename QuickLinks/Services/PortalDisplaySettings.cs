namespace QuickLinks.Services;

/// <summary>Qué muestra el portal público y cuánto recuerda el navegador de cada usuario.</summary>
public sealed record PortalDisplaySettings(
    string SearchPlaceholder,
    bool ShowFilters,
    bool ShowAppCount,
    bool ShowQuickAccess,
    bool OpenInNewTab,
    int RecentMax,
    int RecentDays,
    int PinnedDays,
    int SearchDelayMs);
