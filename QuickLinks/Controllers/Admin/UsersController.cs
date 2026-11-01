using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;
using QuickLinks.Services;
using QuickLinks.Services.Auth;

namespace QuickLinks.Controllers.Admin;

public class UsersController(AppDbContext db, IAuthService auth, ISettingsService settings) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Users.AsNoTracking();
        if (query.Term is string term)
        {
            source = source.Where(u => u.Name.Contains(term) || u.UserName.Contains(term));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("username", false) => source.OrderBy(u => u.UserName),
            ("username", true) => source.OrderByDescending(u => u.UserName),
            ("login", false) => source.OrderBy(u => u.LastLogin),
            ("login", true) => source.OrderByDescending(u => u.LastLogin),
            ("status", false) => source.OrderBy(u => u.Available).ThenBy(u => u.Name),
            ("status", true) => source.OrderByDescending(u => u.Available).ThenBy(u => u.Name),
            (_, true) => source.OrderByDescending(u => u.Name),
            _ => source.OrderBy(u => u.Name)
        };

        var now = DateTime.UtcNow;
        var rows = source.Select(u => new UserRow(
            u.Id, u.Name, u.UserName, u.Role.Name, u.LastLogin, u.LockoutEnd > now, u.Available));

        return View(await PagedList<UserRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct) =>
        View("Form", new UserForm { Roles = await RoleOptionsAsync(ct) });

    [HttpPost]
    public async Task<IActionResult> Create(UserForm form, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(form.Password))
        {
            ModelState.AddModelError(nameof(form.Password), "La contraseña es obligatoria para un usuario nuevo.");
        }

        if (!await ValidateAsync(form, ct))
        {
            form.Roles = await RoleOptionsAsync(ct);
            return View("Form", form);
        }

        db.Users.Add(new User
        {
            Name = form.Name.Trim(),
            UserName = form.UserName.Trim(),
            RoleId = form.RoleId,
            PasswordHash = auth.HashPassword(form.Password!),
            Available = form.Available,
            UpdatedByUserId = CurrentUserId
        });
        return await SaveAsync(form, "Usuario creado.", ct);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return NotFound();
        }

        return View("Form", new UserForm
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName,
            RoleId = user.RoleId,
            Available = user.Available,
            Roles = await RoleOptionsAsync(ct)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserForm form, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return NotFound();
        }

        form.Id = id;
        var losesAdmin = user.Available && user.RoleId == Role.AdministratorId
            && (!form.Available || form.RoleId != Role.AdministratorId);
        if (losesAdmin && !await AnotherActiveAdminExistsAsync(id, ct))
        {
            ModelState.AddModelError(string.Empty, "Debe quedar al menos un administrador activo.");
        }

        if (!await ValidateAsync(form, ct))
        {
            form.Roles = await RoleOptionsAsync(ct);
            return View("Form", form);
        }

        user.Name = form.Name.Trim();
        user.UserName = form.UserName.Trim();
        user.RoleId = form.RoleId;
        user.Available = form.Available;
        user.UpdatedByUserId = CurrentUserId;
        if (!string.IsNullOrEmpty(form.Password))
        {
            user.PasswordHash = auth.HashPassword(form.Password);
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
        }

        return await SaveAsync(form, "Usuario actualizado.", ct);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Available && user.RoleId == Role.AdministratorId && !await AnotherActiveAdminExistsAsync(id, ct))
        {
            Notify("No se puede desactivar: debe quedar al menos un administrador activo.");
            return RedirectToAction(nameof(Index));
        }

        user.Available = !user.Available;
        user.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(user.Available ? "Usuario activado." : "Usuario desactivado.");
        return RedirectToAction(nameof(Index));
    }

    private Task<bool> AnotherActiveAdminExistsAsync(int excludedUserId, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Id != excludedUserId && u.Available && u.RoleId == Role.AdministratorId, ct);

    private async Task<bool> ValidateAsync(UserForm form, CancellationToken ct)
    {
        var minLength = (await settings.GetAsync(ct)).Security.PasswordMinLength;
        if (!string.IsNullOrEmpty(form.Password) && form.Password.Length < minLength)
        {
            ModelState.AddModelError(nameof(form.Password), $"La contraseña debe tener al menos {minLength} caracteres.");
        }

        if (!ModelState.IsValid)
        {
            return false;
        }

        var userName = form.UserName.Trim();
        if (await db.Users.AnyAsync(u => u.UserName == userName && u.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.UserName), "Ese usuario ya existe.");
        }

        if (!await db.Roles.AnyAsync(r => r.Id == form.RoleId, ct))
        {
            ModelState.AddModelError(nameof(form.RoleId), "El rol no existe.");
        }

        return ModelState.IsValid;
    }

    private async Task<IActionResult> SaveAsync(UserForm form, string message, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            ModelState.AddModelError(nameof(form.UserName), "Ese usuario ya existe.");
            form.Roles = await RoleOptionsAsync(ct);
            return View("Form", form);
        }

        Notify(message);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IReadOnlyList<SelectListItem>> RoleOptionsAsync(CancellationToken ct) =>
        await db.Roles
            .AsNoTracking()
            .Where(r => r.Available)
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem(r.Name, r.Id.ToString()))
            .ToListAsync(ct);
}
