using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.ViewModels.Portal;

namespace QuickLinks.Services;

/// <summary>
/// Arma el portal público. Carga una sola vez las aplicaciones activas con sus relaciones activas
/// y aplica los filtros en memoria; el catálogo de un cliente es de cientos de registros, no de miles.
/// </summary>
public sealed class PortalService(AppDbContext db, ISettingsService settings, IMediaStorage media) : IPortalService
{
    private sealed record AppRow(
        int Id,
        string Name,
        string Link,
        string? Image,
        List<int> CategoryIds,
        List<int> BuildingIds,
        List<int> GroupIds);

    public async Task<PortalViewModel> BuildAsync(
        PortalFilter filter,
        IReadOnlyList<int> pinnedIds,
        IReadOnlyList<int> recentIds,
        CancellationToken ct = default)
    {
        var portalSettings = await settings.GetAsync(ct);
        var now = DateTime.UtcNow;

        var apps = await db.Applications
            .AsNoTracking()
            .Where(a => a.Available)
            .OrderBy(a => a.Name)
            .Select(a => new AppRow(
                a.Id,
                a.Name,
                a.Link,
                a.Image,
                a.Categories.Where(c => c.Available && c.Category.Available).Select(c => c.CategoryId).ToList(),
                a.Locations.Where(l => l.Available && l.Building.Available && l.Building.Site.Available).Select(l => l.BuildingId).ToList(),
                a.Groups.Where(g => g.Available && g.Group.Available).Select(g => g.GroupId).ToList()))
            .ToListAsync(ct);

        var maintenances = await db.Maintenances
            .AsNoTracking()
            .Where(m => m.Available && m.DisableApplication && m.Application.Available)
            .Where(m => now > m.StartDate && (m.EndDate == null || now < m.EndDate))
            .Select(m => new { m.ApplicationId, m.CustomMessage, ApplicationName = m.Application.Name })
            .ToListAsync(ct);

        var advices = await db.Advices
            .AsNoTracking()
            .Where(a => a.Available && now > a.StartDate && (a.EndDate == null || now < a.EndDate))
            .OrderBy(a => a.StartDate)
            .Select(a => new { a.Title, a.Content })
            .ToListAsync(ct);

        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.Available)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Color })
            .ToListAsync(ct);

        var buildings = await db.Buildings
            .AsNoTracking()
            .Where(b => b.Available && b.Site.Available)
            .OrderBy(b => b.Site.Name).ThenBy(b => b.Name)
            .Select(b => new FilterOption(b.Id, b.Name, b.SiteId))
            .ToListAsync(ct);

        var sites = await db.Sites
            .AsNoTracking()
            .Where(s => s.Available)
            .OrderBy(s => s.Name)
            .Select(s => new FilterOption(s.Id, s.Name, null))
            .ToListAsync(ct);

        var groups = await db.Groups
            .AsNoTracking()
            .Where(g => g.Available)
            .OrderBy(g => g.Name)
            .Select(g => new FilterOption(g.Id, g.Name, null))
            .ToListAsync(ct);

        // El edificio prevalece sobre el sitio y define a qué sitio pertenece.
        var selectedBuilding = buildings.FirstOrDefault(b => b.Id == filter.BuildingId);
        filter.BuildingId = selectedBuilding?.Id;
        if (selectedBuilding is not null)
        {
            filter.SiteId = selectedBuilding.ParentId;
        }

        var blockedIds = maintenances.Select(m => m.ApplicationId).ToHashSet();
        var appsById = apps.ToDictionary(a => a.Id);

        AppCard ToCard(AppRow app) => new(
            app.Id,
            app.Name,
            app.Link,
            string.IsNullOrEmpty(app.Image) ? null : media.GetUrl(MediaFolder.Logo, app.Image),
            blockedIds.Contains(app.Id));

        var siteBuildingIds = filter.SiteId is int siteId
            ? buildings.Where(b => b.ParentId == siteId).Select(b => b.Id).ToHashSet()
            : null;

        bool MatchesLocation(AppRow app) =>
            filter.BuildingId is int buildingId
                ? app.BuildingIds.Contains(buildingId)
                : siteBuildingIds is null || app.BuildingIds.Any(siteBuildingIds.Contains);

        var search = filter.Search?.Trim() ?? string.Empty;
        bool MatchesSearch(AppRow app) =>
            search.Length == 0 || app.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase);

        var filtered = apps.Where(a => MatchesSearch(a) && MatchesLocation(a)).ToList();
        var selectedCategoryIds = filter.CategoryIds.ToHashSet();

        var sections = new List<PortalSection>();
        var selectedGroup = groups.FirstOrDefault(g => g.Id == filter.GroupId);
        filter.GroupId = selectedGroup?.Id;

        if (selectedGroup is not null)
        {
            var groupApps = filtered
                .Where(a => a.GroupIds.Contains(selectedGroup.Id))
                .Where(a => selectedCategoryIds.Count == 0 || a.CategoryIds.Any(selectedCategoryIds.Contains))
                .Select(ToCard)
                .ToList();
            sections.Add(new PortalSection(selectedGroup.Name, portalSettings.PrimaryColor, groupApps));
        }
        else
        {
            foreach (var category in categories.Where(c => selectedCategoryIds.Count == 0 || selectedCategoryIds.Contains(c.Id)))
            {
                var categoryApps = filtered.Where(a => a.CategoryIds.Contains(category.Id)).Select(ToCard).ToList();
                if (categoryApps.Count > 0)
                {
                    sections.Add(new PortalSection(category.Name, category.Color, categoryApps));
                }
            }
        }

        // Pinned y recientes conservan el orden de la cookie (los recientes, del más nuevo al más viejo).
        List<AppCard> FromIds(IEnumerable<int> ids) =>
            ids.Distinct().Where(appsById.ContainsKey).Select(id => ToCard(appsById[id])).ToList();

        var notices = maintenances
            .Select(m => string.IsNullOrWhiteSpace(m.CustomMessage) ? $"{m.ApplicationName} está en mantenimiento." : m.CustomMessage)
            .Concat(advices.Select(a => $"{a.Title}: {a.Content}"))
            .ToList();

        return new PortalViewModel
        {
            Filter = filter,
            Notices = notices,
            Pinned = FromIds(pinnedIds),
            Recent = FromIds(recentIds),
            Sections = sections,
            AppCount = sections.SelectMany(s => s.Apps).Select(a => a.Id).Distinct().Count(),
            Categories = categories
                .Where(c => apps.Any(a => a.CategoryIds.Contains(c.Id)))
                .Select(c => new FilterOption(c.Id, c.Name))
                .ToList(),
            Sites = sites,
            Buildings = buildings,
            Groups = groups
        };
    }

    public async Task<ApplicationDetails?> GetDetailsAsync(int applicationId, CancellationToken ct = default)
    {
        var app = await db.Applications
            .AsNoTracking()
            .Where(a => a.Id == applicationId && a.Available)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Description,
                a.Sme,
                a.UserManual,
                a.Version,
                a.Link,
                a.Image,
                Images = a.CarouselImages.Where(c => c.Available).OrderBy(c => c.Id).Select(c => c.ImageName).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (app is null)
        {
            return null;
        }

        return new ApplicationDetails(
            app.Id,
            app.Name,
            app.Description,
            app.Sme,
            app.UserManual,
            app.Version,
            app.Link,
            string.IsNullOrEmpty(app.Image) ? null : media.GetUrl(MediaFolder.Logo, app.Image),
            app.Images.Select(i => media.GetUrl(MediaFolder.Carousel, i)).ToList());
    }

    public async Task<string?> GetBlockingMaintenanceMessageAsync(int applicationId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var maintenance = await db.Maintenances
            .AsNoTracking()
            .Where(m => m.ApplicationId == applicationId && m.Available && m.DisableApplication)
            .Where(m => now > m.StartDate && (m.EndDate == null || now < m.EndDate))
            .Select(m => new { m.CustomMessage, ApplicationName = m.Application.Name })
            .FirstOrDefaultAsync(ct);

        if (maintenance is null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(maintenance.CustomMessage)
            ? $"{maintenance.ApplicationName} está en mantenimiento. Gracias por tu comprensión."
            : maintenance.CustomMessage;
    }
}
