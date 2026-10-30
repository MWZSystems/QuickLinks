using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuickLinks.Data;

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

        string Value(string key)
        {
            if (raw.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return SettingKeys.Defaults.First(d => d.Key == key).Value;
        }

        var logo = raw.GetValueOrDefault(SettingKeys.LogoImage);

        return new PortalSettings(
            AppName: Value(SettingKeys.AppName),
            CompanyName: raw.GetValueOrDefault(SettingKeys.CompanyName) ?? string.Empty,
            PrimaryColor: Value(SettingKeys.PrimaryColor),
            LogoUrl: string.IsNullOrWhiteSpace(logo) ? null : media.GetUrl(MediaFolder.Branding, logo),
            LinkValidationRegex: Value(SettingKeys.LinkValidationRegex),
            TimeZone: ResolveTimeZone(Value(SettingKeys.TimeZone)));
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
        foreach (var setting in settings)
        {
            setting.Value = values[setting.Name];
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
