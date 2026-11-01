namespace QuickLinks.Services;

/// <summary>Rangos válidos de los parámetros numéricos; los usan la validación de formularios y la lectura de CT_Settings.</summary>
public static class SettingLimits
{
    public const int RecentMaxMin = 1, RecentMaxMax = 20;
    public const int CookieDaysMin = 1, CookieDaysMax = 730;
    public const int SearchDelayMin = 100, SearchDelayMax = 2000;
    public const int NoticesSpeedMin = 5, NoticesSpeedMax = 120;
    public const int MaxFailedMin = 3, MaxFailedMax = 20;
    public const int LockoutMinutesMin = 1, LockoutMinutesMax = 1440;
    public const int SessionMinutesMin = 5, SessionMinutesMax = 720;
    public const int PasswordMinLengthMin = 8, PasswordMinLengthMax = 64;

    // El formulario de aplicaciones admite 30 MB por envío y el de marca 6 MB.
    public const int MediaMaxKbMin = 100, MediaMaxKbMax = 5120;
    public const int CarouselMaxMin = 1, CarouselMaxMax = 15;
}
