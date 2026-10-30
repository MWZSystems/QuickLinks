using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;
using QuickLinks.Models.ViewModels.Admin;
using QuickLinks.Models.ViewModels.Portal;
using QuickLinks.Services;

namespace QuickLinks.Controllers.Admin;

public class ApplicationsController(
    AppDbContext db,
    ISettingsService settings,
    IMediaStorage media,
    ILogger<ApplicationsController> logger) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ListQuery query, CancellationToken ct)
    {
        var source = db.Applications.AsNoTracking();
        if (query.BuildingId is int buildingId)
        {
            source = source.Where(a => a.Locations.Any(l => l.Available && l.BuildingId == buildingId));
        }
        else if (query.SiteId is int siteId)
        {
            source = source.Where(a => a.Locations.Any(l => l.Available && l.Building.SiteId == siteId));
        }

        if (query.Term is string term)
        {
            source = source.Where(a => a.Name.Contains(term) || a.Description.Contains(term) || a.Link.Contains(term));
        }

        source = (query.Sort?.ToLowerInvariant(), query.Desc) switch
        {
            ("updated", false) => source.OrderBy(a => a.LastUpdated),
            ("updated", true) => source.OrderByDescending(a => a.LastUpdated),
            ("status", false) => source.OrderBy(a => a.Available).ThenBy(a => a.Name),
            ("status", true) => source.OrderByDescending(a => a.Available).ThenBy(a => a.Name),
            (_, true) => source.OrderByDescending(a => a.Name),
            _ => source.OrderBy(a => a.Name)
        };

        var rows = source.Select(a => new ApplicationRow(
            a.Id,
            a.Name,
            a.Image,
            a.Link,
            a.Version,
            a.Categories.Where(c => c.Available).Select(c => c.Category.Name).OrderBy(n => n).ToList(),
            a.LastUpdated,
            a.Available));

        ViewBag.Options = await LoadOptionsAsync(ct);
        return View(await PagedList<ApplicationRow>.CreateAsync(rows, query, ct));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var options = await LoadOptionsAsync(ct);
        return View("Form", new ApplicationForm
        {
            // Igual que el original: una aplicación nueva queda disponible en todos los edificios activos.
            BuildingIds = options.Buildings.Select(b => b.Id).ToList(),
            Options = options
        });
    }

    [HttpPost]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Create(ApplicationForm form, CancellationToken ct)
    {
        if (!await ValidateAsync(form, existingCarouselCount: 0, ct))
        {
            return await RedisplayAsync(form, null, ct);
        }

        var app = new Application { UpdatedByUserId = CurrentUserId };
        db.Applications.Add(app);
        return await SaveAsync(app, form, isNew: true, ct);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var app = await LoadAsync(id, ct);
        if (app is null)
        {
            return NotFound();
        }

        var form = new ApplicationForm
        {
            Id = app.Id,
            Name = app.Name,
            Link = app.Link,
            Description = app.Description,
            Version = app.Version,
            Sme = app.Sme,
            UserManual = app.UserManual,
            Available = app.Available,
            CategoryIds = app.Categories.Where(c => c.Available).Select(c => c.CategoryId).ToList(),
            BuildingIds = app.Locations.Where(l => l.Available).Select(l => l.BuildingId).ToList(),
            GroupIds = app.Groups.Where(g => g.Available).Select(g => g.GroupId).ToList()
        };
        return await RedisplayAsync(form, app, ct);
    }

    [HttpPost]
    [RequestSizeLimit(30 * 1024 * 1024)]
    public async Task<IActionResult> Edit(int id, ApplicationForm form, CancellationToken ct)
    {
        var app = await LoadAsync(id, ct, tracking: true);
        if (app is null)
        {
            return NotFound();
        }

        form.Id = id;
        var remaining = app.CarouselImages.Count(c => c.Available && !form.RemoveCarouselIds.Contains(c.Id));
        if (!await ValidateAsync(form, remaining, ct))
        {
            return await RedisplayAsync(form, app, ct);
        }

        app.UpdatedByUserId = CurrentUserId;
        return await SaveAsync(app, form, isNew: false, ct);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAvailable(int id, CancellationToken ct)
    {
        var app = await db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (app is null)
        {
            return NotFound();
        }

        app.Available = !app.Available;
        app.UpdatedByUserId = CurrentUserId;
        await db.SaveChangesAsync(ct);
        Notify(app.Available ? "Aplicación activada." : "Aplicación desactivada.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Guarda los archivos nuevos, aplica los cambios en un solo SaveChanges y, solo si tuvo éxito,
    /// elimina los archivos reemplazados. Si falla, borra los archivos recién subidos.
    /// </summary>
    private async Task<IActionResult> SaveAsync(Application app, ApplicationForm form, bool isNew, CancellationToken ct)
    {
        var uploaded = new List<(MediaFolder Folder, string Name)>();
        var obsolete = new List<(MediaFolder Folder, string Name)>();

        try
        {
            if (form.Logo is { Length: > 0 })
            {
                var (fileName, error) = await media.SaveImageAsync(form.Logo, MediaFolder.Logo, ct);
                if (error is not null)
                {
                    ModelState.AddModelError(nameof(form.Logo), error);
                    return await RedisplayAsync(form, isNew ? null : app, ct);
                }

                uploaded.Add((MediaFolder.Logo, fileName!));
                if (!string.IsNullOrEmpty(app.Image))
                {
                    obsolete.Add((MediaFolder.Logo, app.Image));
                }

                app.Image = fileName;
            }
            else if (form.RemoveLogo && !string.IsNullOrEmpty(app.Image))
            {
                obsolete.Add((MediaFolder.Logo, app.Image));
                app.Image = null;
            }

            foreach (var file in form.CarouselFiles.Where(f => f.Length > 0))
            {
                var (fileName, error) = await media.SaveImageAsync(file, MediaFolder.Carousel, ct);
                if (error is not null)
                {
                    ModelState.AddModelError(nameof(form.CarouselFiles), $"{file.FileName}: {error}");
                    DeleteFiles(uploaded);
                    return await RedisplayAsync(form, isNew ? null : app, ct);
                }

                uploaded.Add((MediaFolder.Carousel, fileName!));
                app.CarouselImages.Add(new CarouselImage { ImageName = fileName!, UpdatedByUserId = CurrentUserId });
            }

            foreach (var image in app.CarouselImages.Where(c => c.Available && form.RemoveCarouselIds.Contains(c.Id)))
            {
                image.Available = false;
                image.UpdatedByUserId = CurrentUserId;
                obsolete.Add((MediaFolder.Carousel, image.ImageName));
            }

            app.Name = form.Name.Trim();
            app.Link = form.Link.Trim();
            app.Description = form.Description.Trim();
            app.Version = NullIfEmpty(form.Version);
            app.Sme = NullIfEmpty(form.Sme);
            app.UserManual = NullIfEmpty(form.UserManual);
            app.Available = form.Available;

            SyncRelations(app.Categories, form.CategoryIds, r => r.CategoryId, id => new CategoryApplication { CategoryId = id });
            SyncRelations(app.Locations, form.BuildingIds, r => r.BuildingId, id => new ApplicationLocation { BuildingId = id });
            SyncRelations(app.Groups, form.GroupIds, r => r.GroupId, id => new ApplicationGroup { GroupId = id });

            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            DeleteFiles(uploaded);
            ModelState.AddModelError(nameof(form.Name), "Ya existe una aplicación con ese nombre.");
            return await RedisplayAsync(form, isNew ? null : app, ct);
        }
        catch
        {
            DeleteFiles(uploaded);
            throw;
        }

        DeleteFiles(obsolete);
        logger.LogInformation("Aplicación {ApplicationId} guardada por {UserId}", app.Id, CurrentUserId);
        Notify(isNew ? "Aplicación creada." : "Aplicación actualizada.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Reactiva las relaciones seleccionadas, desactiva las omitidas y agrega las nuevas; nunca borra historia.</summary>
    private void SyncRelations<TRelation>(
        ICollection<TRelation> existing,
        IEnumerable<int> selectedIds,
        Func<TRelation, int> key,
        Func<int, TRelation> create)
        where TRelation : AuditedEntity
    {
        var selected = selectedIds.ToHashSet();
        foreach (var relation in existing)
        {
            var shouldBeActive = selected.Remove(key(relation));
            if (relation.Available != shouldBeActive)
            {
                relation.Available = shouldBeActive;
                relation.UpdatedByUserId = CurrentUserId;
            }
        }

        foreach (var id in selected)
        {
            var relation = create(id);
            relation.UpdatedByUserId = CurrentUserId;
            existing.Add(relation);
        }
    }

    private async Task<bool> ValidateAsync(ApplicationForm form, int existingCarouselCount, CancellationToken ct)
    {
        form.CategoryIds = form.CategoryIds.Distinct().ToList();
        form.BuildingIds = form.BuildingIds.Distinct().ToList();
        form.GroupIds = form.GroupIds.Distinct().ToList();

        if (form.CategoryIds.Count == 0)
        {
            ModelState.AddModelError(nameof(form.CategoryIds), "Selecciona al menos una categoría.");
        }

        if (form.BuildingIds.Count == 0)
        {
            ModelState.AddModelError(nameof(form.BuildingIds), "Selecciona al menos un edificio.");
        }

        var newImages = form.CarouselFiles.Count(f => f.Length > 0);
        if (existingCarouselCount + newImages > ApplicationForm.MaxCarouselImages)
        {
            ModelState.AddModelError(nameof(form.CarouselFiles), $"El carrusel admite hasta {ApplicationForm.MaxCarouselImages} imágenes.");
        }

        if (!string.IsNullOrWhiteSpace(form.Link))
        {
            var portal = await settings.GetAsync(ct);
            if (!IsValidLink(form.Link.Trim(), portal.LinkValidationRegex))
            {
                ModelState.AddModelError(nameof(form.Link), "El enlace no tiene un formato válido (por ejemplo https://servidor/app).");
            }
        }

        if (!ModelState.IsValid)
        {
            return false;
        }

        if (await db.Categories.CountAsync(c => form.CategoryIds.Contains(c.Id), ct) != form.CategoryIds.Count
            || await db.Buildings.CountAsync(b => form.BuildingIds.Contains(b.Id), ct) != form.BuildingIds.Count
            || await db.Groups.CountAsync(g => form.GroupIds.Contains(g.Id), ct) != form.GroupIds.Count)
        {
            ModelState.AddModelError(string.Empty, "Alguna categoría, edificio o grupo seleccionado ya no existe.");
        }

        var name = form.Name.Trim();
        if (await db.Applications.AnyAsync(a => a.Name == name && a.Id != form.Id, ct))
        {
            ModelState.AddModelError(nameof(form.Name), "Ya existe una aplicación con ese nombre.");
        }

        return ModelState.IsValid;
    }

    private bool IsValidLink(string link, string pattern)
    {
        try
        {
            return Regex.IsMatch(link, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
        }
        catch (Exception ex) when (ex is ArgumentException or RegexMatchTimeoutException)
        {
            logger.LogWarning(ex, "LINK_VALIDATION_REGEX inválida; se valida solo como URL absoluta");
            return Uri.TryCreate(link, UriKind.Absolute, out _);
        }
    }

    private async Task<IActionResult> RedisplayAsync(ApplicationForm form, Application? app, CancellationToken ct)
    {
        form.Options = await LoadOptionsAsync(ct, form.CategoryIds, form.BuildingIds, form.GroupIds);
        if (app is not null)
        {
            form.CurrentLogoUrl = string.IsNullOrEmpty(app.Image) ? null : media.GetUrl(MediaFolder.Logo, app.Image);
            form.CurrentCarousel = app.CarouselImages
                .Where(c => c.Available && c.Id > 0)
                .OrderBy(c => c.Id)
                .Select(c => (c.Id, media.GetUrl(MediaFolder.Carousel, c.ImageName)))
                .ToList();
        }

        return View("Form", form);
    }

    private Task<Application?> LoadAsync(int id, CancellationToken ct, bool tracking = false)
    {
        var query = db.Applications
            .Include(a => a.Categories)
            .Include(a => a.Locations)
            .Include(a => a.Groups)
            .Include(a => a.CarouselImages)
            .AsSplitQuery();

        return (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    /// <summary>Opciones activas, más las inactivas que ya estén seleccionadas para no perderlas al editar.</summary>
    private async Task<ApplicationFormOptions> LoadOptionsAsync(
        CancellationToken ct,
        IReadOnlyCollection<int>? categoryIds = null,
        IReadOnlyCollection<int>? buildingIds = null,
        IReadOnlyCollection<int>? groupIds = null)
    {
        categoryIds ??= [];
        buildingIds ??= [];
        groupIds ??= [];

        return new ApplicationFormOptions
        {
            Categories = await db.Categories.AsNoTracking()
                .Where(c => c.Available || categoryIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .Select(c => new FilterOption(c.Id, c.Name, null))
                .ToListAsync(ct),
            Sites = await db.Sites.AsNoTracking()
                .Where(s => s.Available || s.Buildings.Any(b => buildingIds.Contains(b.Id)))
                .OrderBy(s => s.Name)
                .Select(s => new FilterOption(s.Id, s.Name, null))
                .ToListAsync(ct),
            Buildings = await db.Buildings.AsNoTracking()
                .Where(b => (b.Available && b.Site.Available) || buildingIds.Contains(b.Id))
                .OrderBy(b => b.Name)
                .Select(b => new FilterOption(b.Id, b.Name, b.SiteId))
                .ToListAsync(ct),
            Groups = await db.Groups.AsNoTracking()
                .Where(g => g.Available || groupIds.Contains(g.Id))
                .OrderBy(g => g.Name)
                .Select(g => new FilterOption(g.Id, g.Name, null))
                .ToListAsync(ct)
        };
    }

    private void DeleteFiles(IEnumerable<(MediaFolder Folder, string Name)> files)
    {
        foreach (var (folder, name) in files)
        {
            try
            {
                media.Delete(folder, name);
            }
            catch (IOException ex)
            {
                logger.LogWarning(ex, "No se pudo borrar el archivo {Folder}/{FileName}", folder, name);
            }
        }
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
