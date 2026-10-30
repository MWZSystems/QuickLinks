using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;
using QuickLinks.Services;

namespace QuickLinks.Controllers.Admin;

public class AdvicesController(AppDbContext db, ISettingsService settings) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Advices.AsNoTracking();
        if (query.Term is string term)
        {
            source = source.Where(a => a.Title.Contains(term) || a.Content.Contains(term));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("title", false) => source.OrderBy(a => a.Title),
            ("title", true) => source.OrderByDescending(a => a.Title),
            ("start", false) => source.OrderBy(a => a.StartDate),
            (_, _) => source.OrderByDescending(a => a.StartDate)
        };

        var rows = source.Select(a => new ScheduleRow(a.Id, a.Title, a.Content, a.StartDate, a.EndDate, false, a.Available));
        return View(await PagedList<ScheduleRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var portal = await settings.GetAsync(ct);
        var start = portal.LocalNow.AddMinutes(5);
        return View("Form", new AdviceForm { StartDate = start.AddSeconds(-start.Second) });
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdviceForm form, CancellationToken ct)
    {
        var portal = await settings.GetAsync(ct);
        if (!await ValidateAsync(form, portal, ct))
        {
            return View("Form", form);
        }

        db.Advices.Add(new Advice
        {
            Title = form.Title.Trim(),
            Content = form.Content.Trim(),
            StartDate = portal.ToUtc(form.StartDate!.Value),
            EndDate = form.EndDate is DateTime end ? portal.ToUtc(end) : null,
            Available = form.Available,
            UpdatedByUserId = CurrentUserId
        });
        return await SaveAsync(form, "Aviso creado.", ct);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var advice = await db.Advices.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (advice is null)
        {
            return NotFound();
        }

        var portal = await settings.GetAsync(ct);
        return View("Form", new AdviceForm
        {
            Id = advice.Id,
            Title = advice.Title,
            Content = advice.Content,
            StartDate = portal.ToLocal(advice.StartDate),
            EndDate = advice.EndDate is DateTime end ? portal.ToLocal(end) : null,
            Available = advice.Available
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AdviceForm form, CancellationToken ct)
    {
        var advice = await db.Advices.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (advice is null)
        {
            return NotFound();
        }

        form.Id = id;
        var portal = await settings.GetAsync(ct);
        if (!await ValidateAsync(form, portal, ct))
        {
            return View("Form", form);
        }

        advice.Title = form.Title.Trim();
        advice.Content = form.Content.Trim();
        advice.StartDate = portal.ToUtc(form.StartDate!.Value);
        advice.EndDate = form.EndDate is DateTime end ? portal.ToUtc(end) : null;
        advice.Available = form.Available;
        advice.UpdatedByUserId = CurrentUserId;
        return await SaveAsync(form, "Aviso actualizado.", ct);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var advice = await db.Advices.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (advice is null)
        {
            return NotFound();
        }

        advice.Available = !advice.Available;
        advice.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(advice.Available ? "Aviso activado." : "Aviso desactivado.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateAsync(AdviceForm form, PortalSettings portal, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        var now = portal.LocalNow;
        if (form.Id == 0 && form.StartDate < now.Date)
        {
            ModelState.AddModelError(nameof(form.StartDate), "El inicio no puede ser anterior a hoy.");
        }

        if (form.Available && form.EndDate is DateTime end && end <= now)
        {
            ModelState.AddModelError(nameof(form.EndDate), "La fecha de fin ya pasó; desactiva el aviso o cambia la fecha.");
        }

        var title = form.Title.Trim();
        if (await db.Advices.AnyAsync(a => a.Title == title && a.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.Title), "Ya existe un aviso con ese título.");
        }

        return ModelState.IsValid;
    }

    private async Task<IActionResult> SaveAsync(AdviceForm form, string message, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(form.Title), "Ya existe un aviso con ese título.");
            return View("Form", form);
        }

        Notify(message);
        return RedirectToAction(nameof(Index));
    }
}
