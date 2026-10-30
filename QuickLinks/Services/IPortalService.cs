using QuickLinks.Models.ViewModels.Portal;

namespace QuickLinks.Services;

public interface IPortalService
{
    Task<PortalViewModel> BuildAsync(
        PortalFilter filter,
        IReadOnlyList<int> pinnedIds,
        IReadOnlyList<int> recentIds,
        CancellationToken ct = default);

    Task<ApplicationDetails?> GetDetailsAsync(int applicationId, CancellationToken ct = default);

    /// <summary>Mensaje del mantenimiento vigente que bloquea la aplicación, o nulo si no hay.</summary>
    Task<string?> GetBlockingMaintenanceMessageAsync(int applicationId, CancellationToken ct = default);
}
