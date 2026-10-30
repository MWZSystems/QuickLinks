using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;
using QuickLinks.Services;

namespace QuickLinks.Controllers.Admin;

public class MaintenancesController(AppDbContext db, ISettingsService settings) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Maintenances.AsNoTracking();
        if (query.Term is string term)
        {
            source = source.Where(m => m.Application.Name.Contains(term) || (m.CustomMessage != null && m.CustomMessage.Contains(term)));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("app", false) => source.OrderBy(m => m.Application.Name),
            ("app", true) => source.OrderByDescending(m => m.Application.Name),
            ("start", false) => source.OrderBy(m => m.StartDate),
            (_, _) => source.OrderByDescending(m => m.StartDate)
        };

        var rows = source.Select(m => new ScheduleRow(
            m.Id, m.Application.Name, m.CustomMessage, m.StartDate, m.EndDate, m.DisableApplication, m.Available));
        return View(await PagedList<ScheduleRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? applicationId, CancellationToken ct)
    {
        var portal = await settings.GetAsync(ct);
        var start = portal.LocalNow.AddMinutes(5);
        return View("Form", new MaintenanceForm
        {
            ApplicationId = applicationId ?? 0,
            StartDate = start.AddSeconds(-start.Second),
            Applications = await ApplicationOptionsAsync(ct)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(MaintenanceForm form, CancellationToken ct)
    {
        var portal = await settings.GetAsync(ct);
        if (!await ValidateAsync(form, portal, ct))
        {
            form.Applications = await ApplicationOptionsAsync(ct);
            return View("Form", form);
        }

        db.Maintenances.Add(new Maintenance
        {
            ApplicationId = form.ApplicationId,
            CustomMessage = await BuildMessageAsync(form, ct),
            StartDate = portal.ToUtc(form.StartDate!.Value),
            EndDate = form.EndDate is DateTime end ? portal.ToUtc(end) : null,
            DisableApplication = form.DisableApplication,
            Available = form.Available,
            UpdatedByUserId = CurrentUserId
        });
        await db.SaveChangesAsync(ct);
        Notify("Mantenimiento programado.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var maintenance = await db.Maintenances.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
        if (maintenance is null)
        {
            return NotFound();
        }

        var portal = await settings.GetAsync(ct);
        return View("Form", new MaintenanceForm
        {
            Id = maintenance.Id,
            ApplicationId = maintenance.ApplicationId,
            CustomMessage = maintenance.CustomMessage,
            StartDate = portal.ToLocal(maintenance.StartDate),
            EndDate = maintenance.EndDate is DateTime end ? portal.ToLocal(end) : null,
            DisableApplication = maintenance.DisableApplication,
            Available = maintenance.Available,
            Applications = await ApplicationOptionsAsync(ct)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, MaintenanceForm form, CancellationToken ct)
    {
        var maintenance = await db.Maintenances.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (maintenance is null)
        {
            return NotFound();
        }

        form.Id = id;
        var portal = await settings.GetAsync(ct);
        if (!await ValidateAsync(form, portal, ct))
        {
            form.Applications = await ApplicationOptionsAsync(ct);
            return View("Form", form);
        }

        maintenance.ApplicationId = form.ApplicationId;
        maintenance.CustomMessage = await BuildMessageAsync(form, ct);
        maintenance.StartDate = portal.ToUtc(form.StartDate!.Value);
        maintenance.EndDate = form.EndDate is DateTime end ? portal.ToUtc(end) : null;
        maintenance.DisableApplication = form.DisableApplication;
        maintenance.Available = form.Available;
        maintenance.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify("Mantenimiento actualizado.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var maintenance = await db.Maintenances.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (maintenance is null)
        {
            return NotFound();
        }

        maintenance.Available = !maintenance.Available;
        maintenance.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(maintenance.Available ? "Mantenimiento activado." : "Mantenimiento desactivado.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ValidateAsync(MaintenanceForm form, PortalSettings portal, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return false;
        }

        if (!await db.Applications.AnyAsync(a => a.Id == form.ApplicationId, ct))
        {
            ModelState.AddModelError(nameof(form.ApplicationId), "La aplicación no existe.");
            return false;
        }

        var now = portal.LocalNow;
        if (form.Id == 0 && form.StartDate < now.Date)
        {
            ModelState.AddModelError(nameof(form.StartDate), "El inicio no puede ser anterior a hoy.");
        }

        if (form.Available && form.EndDate is DateTime end && end <= now)
        {
            ModelState.AddModelError(nameof(form.EndDate), "La fecha de fin ya pasó; desactiva el mantenimiento o cambia la fecha.");
        }

        var message = form.CustomMessage?.Trim();
        if (!string.IsNullOrEmpty(message) && await db.Maintenances.AnyAsync(
                m => m.ApplicationId == form.ApplicationId && m.CustomMessage == message && m.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.CustomMessage), "Ya existe un mantenimiento con ese mensaje para la aplicación.");
        }

        return ModelState.IsValid;
    }

    /// <summary>Si no se escribió mensaje, genera uno con el nombre de la aplicación y la ventana de tiempo.</summary>
    private async Task<string> BuildMessageAsync(MaintenanceForm form, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(form.CustomMessage))
        {
            return form.CustomMessage.Trim();
        }

        var name = await db.Applications.Where(a => a.Id == form.ApplicationId).Select(a => a.Name).FirstAsync(ct);
        var start = form.StartDate!.Value.ToString("dd/MM/yyyy HH:mm");
        return form.EndDate is DateTime end
            ? $"{name} estará en mantenimiento del {start} al {end:dd/MM/yyyy HH:mm}."
            : $"{name} estará en mantenimiento a partir del {start}.";
    }

    private async Task<IReadOnlyList<SelectListItem>> ApplicationOptionsAsync(CancellationToken ct) =>
        await db.Applications
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new SelectListItem(a.Available ? a.Name : a.Name + " (inactiva)", a.Id.ToString()))
            .ToListAsync(ct);
}
