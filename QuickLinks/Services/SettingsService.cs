using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using L = QuickLinks.Services.SettingLimits;

namespace QuickLinks.Services;

/// <summary>Lee CT_Settings con caché en memoria; la caché se invalida al guardar.</summary>
public sealed class SettingsService(
    AppDbContext db,
    IMemoryCache cache,
    IMediaStorage media,
    ILogger<SettingsService> logger) : ISettingsService
{
    private const string CacheKey = "portal-settings";

    public async Task<PortalSettings> GetAsync(CancellationToken ct = default)
    {
        var raw = await GetRawAsync(ct);

        string Value(string key) =>
            raw.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : SettingKeys.DefaultFor(key);

        // Un valor fuera de rango (editado a mano en la base) no rompe el portal: se acota al rango válido.
        int Int(string key, int min, int max) =>
            int.TryParse(Value(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                ? Math.Clamp(number, min, max)
                : int.Parse(SettingKeys.DefaultFor(key), CultureInfo.InvariantCulture);

        bool Bool(string key) =>
            bool.TryParse(Value(key), out var flag) ? flag : bool.Parse(SettingKeys.DefaultFor(key));

        var logo = raw.GetValueOrDefault(SettingKeys.LogoImage);

        return new PortalSettings(
            AppName: Value(SettingKeys.AppName),
            CompanyName: raw.GetValueOrDefault(SettingKeys.CompanyName) ?? string.Empty,
            PrimaryColor: Value(SettingKeys.PrimaryColor),
            LogoUrl: string.IsNullOrWhiteSpace(logo) ? null : media.GetUrl(MediaFolder.Branding, logo),
            LinkValidationRegex: Value(SettingKeys.LinkValidationRegex),
            TimeZone: ResolveTimeZone(Value(SettingKeys.TimeZone)),
            Portal: new PortalDisplaySettings(
                SearchPlaceholder: Value(SettingKeys.SearchPlaceholder),
                ShowFilters: Bool(SettingKeys.ShowFilters),
                ShowAppCount: Bool(SettingKeys.ShowAppCount),
                ShowQuickAccess: Bool(SettingKeys.ShowQuickAccess),
                OpenInNewTab: Bool(SettingKeys.OpenInNewTab),
                RecentMax: Int(SettingKeys.RecentMax, L.RecentMaxMin, L.RecentMaxMax),
                RecentDays: Int(SettingKeys.RecentDays, L.CookieDaysMin, L.CookieDaysMax),
                PinnedDays: Int(SettingKeys.PinnedDays, L.CookieDaysMin, L.CookieDaysMax),
                SearchDelayMs: Int(SettingKeys.SearchDelayMs, L.SearchDelayMin, L.SearchDelayMax)),
            Notices: new NoticeSettings(
                Label: Value(SettingKeys.NoticesLabel),
                Color: Value(SettingKeys.NoticesColor),
                SpeedSeconds: Int(SettingKeys.NoticesSpeedSeconds, L.NoticesSpeedMin, L.NoticesSpeedMax)),
            Security: new SecuritySettings(
                MaxFailedAttempts: Int(SettingKeys.MaxFailedAttempts, L.MaxFailedMin, L.MaxFailedMax),
                LockoutMinutes: Int(SettingKeys.LockoutMinutes, L.LockoutMinutesMin, L.LockoutMinutesMax),
                SessionMinutes: Int(SettingKeys.SessionMinutes, L.SessionMinutesMin, L.SessionMinutesMax),
                PasswordMinLength: Int(SettingKeys.PasswordMinLength, L.PasswordMinLengthMin, L.PasswordMinLengthMax)),
            Media: new MediaSettings(
                MaxFileSizeKb: Int(SettingKeys.MediaMaxSizeKb, L.MediaMaxKbMin, L.MediaMaxKbMax),
                CarouselMaxImages: Int(SettingKeys.CarouselMaxImages, L.CarouselMaxMin, L.CarouselMaxMax)));
    }

    public async Task<IReadOnlyDictionary<string, string>> GetRawAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyDictionary<string, string>? cached) && cached is not null)
        {
            return cached;
        }

        var values = await db.Settings
            .AsNoTracking()
            .Where(s => s.Available)
            .OrderBy(s => s.SortOrder)
            .ToDictionaryAsync(s => s.Name, s => s.Value, ct);

        cache.Set(CacheKey, (IReadOnlyDictionary<string, string>)values, TimeSpan.FromMinutes(5));
        return values;
    }

    public async Task SaveAsync(IReadOnlyDictionary<string, string> values, int userId, CancellationToken ct = default)
    {
        var settings = await db.Settings.Where(s => values.Keys.Contains(s.Name)).ToListAsync(ct);
        foreach (var (name, value) in values)
        {
            var setting = settings.FirstOrDefault(s => s.Name == name);
            if (setting is null)
            {
                // Claves agregadas después de la siembra inicial: se crean la primera vez que se guardan.
                var order = SettingKeys.Defaults.Select(d => d.Key).ToList().IndexOf(name) + 1;
                setting = new Setting { Name = name, SortOrder = order > 0 ? order : SettingKeys.Defaults.Count + 1 };
                db.Settings.Add(setting);
            }

            setting.Value = value;
            setting.Available = true;
            setting.UpdatedByUserId = userId;
        }

        await db.SaveChangesAsync(ct);
        cache.Remove(CacheKey);
    }

    private TimeZoneInfo ResolveTimeZone(string id)
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
        {
            return zone;
        }

        logger.LogWarning("Zona horaria {TimeZone} no reconocida; se usa UTC", id);
        return TimeZoneInfo.Utc;
    }
}
