namespace QuickLinks.Services;

/// <summary>Nombres de los parámetros de CT_Settings y sus valores iniciales.</summary>
public static class SettingKeys
{
    // Marca
    public const string AppName = "APP_NAME";
    public const string CompanyName = "COMPANY_NAME";
    public const string PrimaryColor = "PRIMARY_COLOR";
    public const string LogoImage = "LOGO_IMAGE";

    // Regional
    public const string LinkValidationRegex = "LINK_VALIDATION_REGEX";
    public const string TimeZone = "TIME_ZONE";

    // Portal
    public const string SearchPlaceholder = "PORTAL_SEARCH_PLACEHOLDER";
    public const string ShowFilters = "PORTAL_SHOW_FILTERS";
    public const string ShowAppCount = "PORTAL_SHOW_APP_COUNT";
    public const string ShowQuickAccess = "PORTAL_SHOW_QUICK_ACCESS";
    public const string OpenInNewTab = "PORTAL_OPEN_NEW_TAB";
    public const string RecentMax = "PORTAL_RECENT_MAX";
    public const string RecentDays = "PORTAL_RECENT_DAYS";
    public const string PinnedDays = "PORTAL_PINNED_DAYS";
    public const string SearchDelayMs = "PORTAL_SEARCH_DELAY_MS";

    // Avisos
    public const string NoticesLabel = "NOTICES_LABEL";
    public const string NoticesColor = "NOTICES_COLOR";
    public const string NoticesSpeedSeconds = "NOTICES_SPEED_SECONDS";

    // Seguridad
    public const string MaxFailedAttempts = "SECURITY_MAX_FAILED";
    public const string LockoutMinutes = "SECURITY_LOCKOUT_MINUTES";
    public const string SessionMinutes = "SECURITY_SESSION_MINUTES";
    public const string PasswordMinLength = "SECURITY_PASSWORD_MIN";

    // Imágenes
    public const string MediaMaxSizeKb = "MEDIA_MAX_KB";
    public const string CarouselMaxImages = "MEDIA_CAROUSEL_MAX";

    /// <summary>Valores neutros para un cliente nuevo; el orden define SettingOrder.</summary>
    public static readonly IReadOnlyList<KeyValuePair<string, string>> Defaults =
    [
        new(AppName, "QuickLinks"),
        new(CompanyName, ""),
        new(PrimaryColor, "#0D4D84"),
        new(LogoImage, ""),
        new(LinkValidationRegex, "^(https?://).+"),
        new(TimeZone, "America/Mexico_City"),
        new(SearchPlaceholder, "Buscar aplicación..."),
        new(ShowFilters, "true"),
        new(ShowAppCount, "true"),
        new(ShowQuickAccess, "true"),
        new(OpenInNewTab, "true"),
        new(RecentMax, "5"),
        new(RecentDays, "30"),
        new(PinnedDays, "150"),
        new(SearchDelayMs, "300"),
        new(NoticesLabel, "Avisos"),
        new(NoticesColor, "#FCD34D"),
        new(NoticesSpeedSeconds, "30"),
        new(MaxFailedAttempts, "5"),
        new(LockoutMinutes, "15"),
        new(SessionMinutes, "30"),
        new(PasswordMinLength, "12"),
        new(MediaMaxSizeKb, "2048"),
        new(CarouselMaxImages, "10")
    ];

    /// <summary>
    /// Filas que crea la migración InitialSchema. Las claves posteriores no se siembran:
    /// usan su valor por defecto hasta que el administrador las guarda por primera vez.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> InitialSeed => Defaults.Take(6);

    public static string DefaultFor(string key) => Defaults.First(d => d.Key == key).Value;
}
