using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;

namespace QuickLinks.Controllers.Admin;

public class BuildingsController(AppDbContext db) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Buildings.AsNoTracking();
        if (query.SiteId is int siteId)
        {
            source = source.Where(b => b.SiteId == siteId);
        }

        if (query.Term is string term)
        {
            source = source.Where(b => b.Name.Contains(term) || b.Site.Name.Contains(term));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("site", false) => source.OrderBy(b => b.Site.Name).ThenBy(b => b.Name),
            ("site", true) => source.OrderByDescending(b => b.Site.Name).ThenBy(b => b.Name),
            ("updated", false) => source.OrderBy(b => b.LastUpdated),
            ("updated", true) => source.OrderByDescending(b => b.LastUpdated),
            ("status", false) => source.OrderBy(b => b.Available).ThenBy(b => b.Name),
            ("status", true) => source.OrderByDescending(b => b.Available).ThenBy(b => b.Name),
            (_, true) => source.OrderByDescending(b => b.Name),
            _ => source.OrderBy(b => b.Site.Name).ThenBy(b => b.Name)
        };

        var rows = source.Select(b => new NamedRow(
            b.Id, b.Name, b.Site.Name,
            b.Applications.Count(a => a.Available && a.Application.Available),
            b.LastUpdated, b.Available));

        ViewBag.Sites = await SiteOptionsAsync(ct);
        return View(await PagedList<NamedRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? siteId, CancellationToken ct) =>
        View("Form", new BuildingForm { SiteId = siteId ?? 0, Sites = await SiteOptionsAsync(ct) });

    [HttpPost]
    public async Task<IActionResult> Create(BuildingForm form, CancellationToken ct)
    {
        if (!await ValidateAsync(form, ct))
        {
            form.Sites = await SiteOptionsAsync(ct);
            return View("Form", form);
        }

        db.Buildings.Add(new Building
        {
            SiteId = form.SiteId,
            Name = form.Name.Trim(),
            Available = form.Available,
            UpdatedByUserId = CurrentUserId
        });
        return await SaveAsync(form, "Edificio creado.", ct);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var building = await db.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
        if (building is null)
        {
            return NotFound();
        }

        return View("Form", new BuildingForm
        {
            Id = building.Id,
            SiteId = building.SiteId,
            Name = building.Name,
            Available = building.Available,
            Sites = await SiteOptionsAsync(ct)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, BuildingForm form, CancellationToken ct)
    {
        var building = await db.Buildings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (building is null)
        {
            return NotFound();
        }

        form.Id = id;
        if (!await ValidateAsync(form, ct))
        {
            form.Sites = await SiteOptionsAsync(ct);
            return View("Form", form);
        }

        building.SiteId = form.SiteId;
        building.Name = form.Name.Trim();
        building.Available = form.Available;
        building.UpdatedByUserId = CurrentUserId;
        return await SaveAsync(form, "Edificio actualizado.", ct);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var building = await db.Buildings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (building is null)
        {
            return NotFound();
        }

        building.Available = !building.Available;
        building.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(building.Available ? "Edificio activado." : "Edificio desactivado.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateAsync(BuildingForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        if (!await db.Sites.AnyAsync(s => s.Id == form.SiteId, ct))
        {
            ModelState.AddModelError(nameof(form.SiteId), "El sitio no existe.");
            return false;
        }

        var name = form.Name.Trim();
        if (await db.Buildings.AnyAsync(b => b.SiteId == form.SiteId && b.Name == name && b.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.Name), "Ese sitio ya tiene un edificio con ese nombre.");
        }

        return ModelState.IsValid;
    }

    private async Task<IActionResult> SaveAsync(BuildingForm form, string message, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(form.Name), "Ese sitio ya tiene un edificio con ese nombre.");
            form.Sites = await SiteOptionsAsync(ct);
            return View("Form", form);
        }

        Notify(message);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IReadOnlyList<SelectListItem>> SiteOptionsAsync(CancellationToken ct) =>
        await db.Sites
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem(s.Available ? s.Name : s.Name + " (inactivo)", s.Id.ToString()))
            .ToListAsync(ct);
}
