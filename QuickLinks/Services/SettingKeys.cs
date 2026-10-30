namespace QuickLinks.Services;

/// <summary>Nombres de los parámetros de CT_Settings y sus valores iniciales.</summary>
public static class SettingKeys
{
    public const string AppName = "APP_NAME";
    public const string CompanyName = "COMPANY_NAME";
    public const string PrimaryColor = "PRIMARY_COLOR";
    public const string LogoImage = "LOGO_IMAGE";
    public const string LinkValidationRegex = "LINK_VALIDATION_REGEX";
    public const string TimeZone = "TIME_ZONE";

    /// <summary>Valores neutros para un cliente nuevo; el orden define SettingOrder.</summary>
    public static readonly IReadOnlyList<KeyValuePair<string, string>> Defaults =
    [
        new(AppName, "QuickLinks"),
        new(CompanyName, ""),
        new(PrimaryColor, "#0D4D84"),
        new(LogoImage, ""),
        new(LinkValidationRegex, "^(https?://).+"),
        new(TimeZone, "America/Mexico_City")
    ];
}
