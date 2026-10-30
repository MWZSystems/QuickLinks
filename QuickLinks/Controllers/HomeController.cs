using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickLinks.Models;
using QuickLinks.Models.ViewModels.Portal;
using QuickLinks.Services;

namespace QuickLinks.Controllers;

/// <summary>Portal público de aplicaciones.</summary>
[AllowAnonymous]
public class HomeController(IPortalService portal) : Controller
{
    public const string PinnedCookie = "pinnedApps";
    public const string RecentCookie = "recentlyUsedApps";
    public const string ShowAllCookie = "showAllDefault";

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] PortalFilter filter, CancellationToken ct)
    {
        var model = await BuildAsync(filter, ct);
        return View(model);
    }

    /// <summary>Resultados para la búsqueda en vivo: modo simple, modo completo y contador en un solo fragmento.</summary>
    [HttpGet]
    public async Task<IActionResult> Results([FromQuery] PortalFilter filter, CancellationToken ct)
    {
        var model = await BuildAsync(filter, ct);
        return PartialView("_Results", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var details = await portal.GetDetailsAsync(id, ct);
        return details is null ? NotFound() : Json(details);
    }

    [HttpGet]
    public async Task<IActionResult> Maintenance(int id, CancellationToken ct)
    {
        var message = await portal.GetBlockingMaintenanceMessageAsync(id, ct);
        return message is null ? NotFound() : Json(new { message });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private Task<PortalViewModel> BuildAsync(PortalFilter filter, CancellationToken ct)
    {
        var showAll = filter.ShowAll ?? Request.Cookies[ShowAllCookie] == "true";
        return portal.BuildAsync(filter, showAll, ReadIds(PinnedCookie), ReadIds(RecentCookie), ct);
    }

    private List<int> ReadIds(string cookieName)
    {
        var value = Request.Cookies[cookieName];
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToList();
    }
}
