namespace QuickLinks.Services;

public interface ISettingsService
{
    Task<PortalSettings> GetAsync(CancellationToken ct = default);

    Task<IReadOnlyDictionary<string, string>> GetRawAsync(CancellationToken ct = default);

    Task SaveAsync(IReadOnlyDictionary<string, string> values, int userId, CancellationToken ct = default);
}
