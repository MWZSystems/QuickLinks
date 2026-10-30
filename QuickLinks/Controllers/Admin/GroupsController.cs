using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;

namespace QuickLinks.Controllers.Admin;

public class GroupsController(AppDbContext db) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Groups.AsNoTracking();
        if (query.Term is string term)
        {
            source = source.Where(g => g.Name.Contains(term));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("updated", false) => source.OrderBy(g => g.LastUpdated),
            ("updated", true) => source.OrderByDescending(g => g.LastUpdated),
            ("status", false) => source.OrderBy(g => g.Available).ThenBy(g => g.Name),
            ("status", true) => source.OrderByDescending(g => g.Available).ThenBy(g => g.Name),
            (_, true) => source.OrderByDescending(g => g.Name),
            _ => source.OrderBy(g => g.Name)
        };

        var rows = source.Select(g => new NamedRow(
            g.Id, g.Name, null,
            g.Applications.Count(a => a.Available && a.Application.Available),
            g.LastUpdated, g.Available));

        return View(await PagedList<NamedRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new NamedForm());

    [HttpPost]
    public async Task<IActionResult> Create(NamedForm form, CancellationToken ct)
    {
        if (!await ValidateAsync(form, ct))
        {
            return View("Form", form);
        }

        db.Groups.Add(new Group { Name = form.Name.Trim(), Available = form.Available, UpdatedByUserId = CurrentUserId });
        return await SaveAsync(form, "Grupo creado.", ct);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var group = await db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);
        return group is null
            ? NotFound()
            : View("Form", new NamedForm { Id = group.Id, Name = group.Name, Available = group.Available });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, NamedForm form, CancellationToken ct)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null)
        {
            return NotFound();
        }

        form.Id = id;
        if (!await ValidateAsync(form, ct))
        {
            return View("Form", form);
        }

        group.Name = form.Name.Trim();
        group.Available = form.Available;
        group.UpdatedByUserId = CurrentUserId;
        return await SaveAsync(form, "Grupo actualizado.", ct);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == id, ct);
        if (group is null)
        {
            return NotFound();
        }

        group.Available = !group.Available;
        group.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(group.Available ? "Grupo activado." : "Grupo desactivado.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateAsync(NamedForm form, CancellationToken ct)
    {
        var name = form.Name?.Trim() ?? string.Empty;
        if (ModelState.IsValid && await db.Groups.AnyAsync(g => g.Name == name && g.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.Name), "Ya existe un grupo con ese nombre.");
        }

        return ModelState.IsValid;
    }

    private async Task<IActionResult> SaveAsync(NamedForm form, string message, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(form.Name), "Ya existe un grupo con ese nombre.");
            return View("Form", form);
        }

        Notify(message);
        return RedirectToAction(nameof(Index));
    }
}
