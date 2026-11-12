using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Services;

namespace QuickLinks.Controllers.Admin;

/// <summary>Resumen del portal para quien administra: solo conteos, con las mismas definiciones de «activo» que usa el portal.</summary>
public class SummaryController(AppDbContext db) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var counts = new AdminCounts(
            await db.Applications.AsNoTracking().CountAsync(a => a.Available, ct),
            // Igual que PortalService: mantenimiento vigente que bloquea una aplicación disponible.
            await db.Maintenances.AsNoTracking()
                .Where(m => m.Available && m.DisableApplication && m.Application.Available && now > m.StartDate && (m.EndDate == null || now < m.EndDate))
                .Select(m => m.ApplicationId).Distinct().CountAsync(ct),
            await db.Advices.AsNoTracking().CountAsync(a => a.Available && now > a.StartDate && (a.EndDate == null || now < a.EndDate), ct),
            await db.Maintenances.AsNoTracking().CountAsync(m => m.Available && m.Application.Available && m.StartDate > now, ct),
            await db.Users.AsNoTracking().CountAsync(u => u.Available, ct),
            (await db.CategoryApplications.AsNoTracking()
                .Where(c => c.Available && c.Category.Available && c.Application.Available)
                .GroupBy(c => c.Category.Name).Select(g => new { Name = g.Key, Count = g.Count() }).ToListAsync(ct))
                .Select(x => new CategoryCount(x.Name, x.Count)).ToList());
        return View(counts);
    }
}
