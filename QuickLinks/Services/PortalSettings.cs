namespace QuickLinks.Services;

/// <summary>Instantánea de CT_Settings con los valores ya interpretados.</summary>
public sealed record PortalSettings(
    string AppName,
    string CompanyName,
    string PrimaryColor,
    string? LogoUrl,
    string LinkValidationRegex,
    TimeZoneInfo TimeZone,
    PortalDisplaySettings Portal,
    NoticeSettings Notices,
    SecuritySettings Security,
    MediaSettings Media)
{
    /// <summary>Convierte una fecha UTC de la base de datos a la hora local configurada.</summary>
    public DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TimeZone);

    /// <summary>Convierte una fecha capturada en la hora local configurada a UTC.</summary>
    public DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZone);

    public DateTime LocalNow => ToLocal(DateTime.UtcNow);
}
