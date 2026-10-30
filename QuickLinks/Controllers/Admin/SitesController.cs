using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;

namespace QuickLinks.Controllers.Admin;

public class SitesController(AppDbContext db) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Sites.AsNoTracking();
        if (query.Term is string term)
        {
            source = source.Where(s => s.Name.Contains(term) || s.Address.Contains(term));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("address", false) => source.OrderBy(s => s.Address),
            ("address", true) => source.OrderByDescending(s => s.Address),
            ("updated", false) => source.OrderBy(s => s.LastUpdated),
            ("updated", true) => source.OrderByDescending(s => s.LastUpdated),
            ("status", false) => source.OrderBy(s => s.Available).ThenBy(s => s.Name),
            ("status", true) => source.OrderByDescending(s => s.Available).ThenBy(s => s.Name),
            (_, true) => source.OrderByDescending(s => s.Name),
            _ => source.OrderBy(s => s.Name)
        };

        // ApplicationCount aquí cuenta edificios activos del sitio.
        var rows = source.Select(s => new NamedRow(
            s.Id, s.Name, s.Address,
            s.Buildings.Count(b => b.Available),
            s.LastUpdated, s.Available));

        return View(await PagedList<NamedRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new SiteForm());

    [HttpPost]
    public async Task<IActionResult> Create(SiteForm form, CancellationToken ct)
    {
        if (!await ValidateAsync(form, ct))
        {
            return View("Form", form);
        }

        db.Sites.Add(new Site
        {
            Name = form.Name.Trim(),
            Address = form.Address.Trim(),
            Available = form.Available,
            CreatedByUserId = CurrentUserId,
            CreationDate = DateTime.UtcNow,
            UpdatedByUserId = CurrentUserId
        });
        return await SaveAsync(form, "Sitio creado.", ct);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var site = await db.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        return site is null
            ? NotFound()
            : View("Form", new SiteForm { Id = site.Id, Name = site.Name, Address = site.Address, Available = site.Available });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, SiteForm form, CancellationToken ct)
    {
        var site = await db.Sites.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (site is null)
        {
            return NotFound();
        }

        form.Id = id;
        if (!await ValidateAsync(form, ct))
        {
            return View("Form", form);
        }

        site.Name = form.Name.Trim();
        site.Address = form.Address.Trim();
        site.Available = form.Available;
        site.UpdatedByUserId = CurrentUserId;
        return await SaveAsync(form, "Sitio actualizado.", ct);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var site = await db.Sites.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (site is null)
        {
            return NotFound();
        }

        site.Available = !site.Available;
        site.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(site.Available ? "Sitio activado." : "Sitio desactivado.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateAsync(SiteForm form, CancellationToken ct)
    {
        var name = form.Name?.Trim() ?? string.Empty;
        if (ModelState.IsValid && await db.Sites.AnyAsync(s => s.Name == name && s.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.Name), "Ya existe un sitio con ese nombre.");
        }

        return ModelState.IsValid;
    }

    private async Task<IActionResult> SaveAsync(SiteForm form, string message, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(form.Name), "Ya existe un sitio con ese nombre.");
            return View("Form", form);
        }

        Notify(message);
        return RedirectToAction(nameof(Index));
    }
}
