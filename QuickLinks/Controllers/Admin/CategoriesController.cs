using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;

namespace QuickLinks.Controllers.Admin;

public class CategoriesController(AppDbContext db) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Categories.AsNoTracking();
        if (query.Term is string term)
        {
            source = source.Where(c => c.Name.Contains(term));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("updated", false) => source.OrderBy(c => c.LastUpdated),
            ("updated", true) => source.OrderByDescending(c => c.LastUpdated),
            ("status", false) => source.OrderBy(c => c.Available).ThenBy(c => c.Name),
            ("status", true) => source.OrderByDescending(c => c.Available).ThenBy(c => c.Name),
            (_, true) => source.OrderByDescending(c => c.Name),
            _ => source.OrderBy(c => c.Name)
        };

        var rows = source.Select(c => new CategoryRow(
            c.Id, c.Name, c.Color,
            c.Applications.Count(a => a.Available && a.Application.Available),
            c.LastUpdated, c.Available));

        return View(await PagedList<CategoryRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new CategoryForm());

    [HttpPost]
    public async Task<IActionResult> Create(CategoryForm form, CancellationToken ct)
    {
        if (!await ValidateAsync(form, ct))
        {
            return View("Form", form);
        }

        db.Categories.Add(new Category
        {
            Name = form.Name.Trim(),
            Color = form.Color.ToUpperInvariant(),
            Available = form.Available,
            UpdatedByUserId = CurrentUserId
        });

        return await SaveAsync(form, "Categoría creada.", ct);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var category = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return NotFound();
        }

        return View("Form", new CategoryForm
        {
            Id = category.Id,
            Name = category.Name,
            Color = category.Color,
            Available = category.Available
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CategoryForm form, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return NotFound();
        }

        form.Id = id;
        if (!await ValidateAsync(form, ct))
        {
            return View("Form", form);
        }

        category.Name = form.Name.Trim();
        category.Color = form.Color.ToUpperInvariant();
        category.Available = form.Available;
        category.UpdatedByUserId = CurrentUserId;

        return await SaveAsync(form, "Categoría actualizada.", ct);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (category is null)
        {
            return NotFound();
        }

        category.Available = !category.Available;
        category.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(category.Available ? "Categoría activada." : "Categoría desactivada.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateAsync(CategoryForm form, CancellationToken ct)
    {
        var name = form.Name?.Trim() ?? string.Empty;
        if (ModelState.IsValid && await db.Categories.AnyAsync(c => c.Name == name && c.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.Name), "Ya existe una categoría con ese nombre.");
        }

        return ModelState.IsValid;
    }

    private async Task<IActionResult> SaveAsync(CategoryForm form, string message, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(form.Name), "Ya existe una categoría con ese nombre.");
            return View("Form", form);
        }

        Notify(message);
        return RedirectToAction(nameof(Index));
    }
}
