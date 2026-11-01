using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using QuickLinks.Models.ViewModels.Admin;
using QuickLinks.Services;

namespace QuickLinks.Controllers.Admin;

/// <summary>Menú de configuración: cada apartado edita su grupo de parámetros de CT_Settings.</summary>
public class SettingsController(ISettingsService settings, IMediaStorage media) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var s = await settings.GetAsync(ct);
        static string YesNo(bool value) => value ? "Sí" : "No";

        var summaries = new Dictionary<string, IReadOnlyList<(string, string)>>
        {
            ["Brand"] = [("Nombre", s.AppName), ("Empresa", Or(s.CompanyName, "Sin definir")), ("Color", s.PrimaryColor), ("Logo", s.LogoUrl is null ? "Sin logo" : "Cargado")],
            ["Portal"] = [("Filtros", YesNo(s.Portal.ShowFilters)), ("Contador", YesNo(s.Portal.ShowAppCount)), ("Fijadas y recientes", YesNo(s.Portal.ShowQuickAccess)), ("Pestaña nueva", YesNo(s.Portal.OpenInNewTab))],
            ["Notices"] = [("Etiqueta", s.Notices.Label), ("Color", s.Notices.Color), ("Vuelta", $"{s.Notices.SpeedSeconds} s")],
            ["Security"] = [("Intentos", s.Security.MaxFailedAttempts.ToString(CultureInfo.InvariantCulture)), ("Bloqueo", $"{s.Security.LockoutMinutes} min"), ("Sesión", $"{s.Security.SessionMinutes} min"), ("Contraseña", $"{s.Security.PasswordMinLength} caracteres")],
            ["Media"] = [("Peso máximo", $"{s.Media.MaxFileSizeKb} KB"), ("Carrusel", $"{s.Media.CarouselMaxImages} imágenes")],
            ["Regional"] = [("Zona horaria", s.TimeZone.Id), ("Enlaces", s.LinkValidationRegex)]
        };

        return View(SettingsSection.All.Select(section => new SettingsOverviewItem(section, summaries[section.Action])).ToList());
    }

    // ---------- Marca ----------

    [HttpGet]
    public async Task<IActionResult> Brand(CancellationToken ct)
    {
        var raw = await settings.GetRawAsync(ct);
        var s = await settings.GetAsync(ct);
        return View(new BrandSettingsForm
        {
            AppName = s.AppName,
            CompanyName = raw.GetValueOrDefault(SettingKeys.CompanyName),
            PrimaryColor = s.PrimaryColor,
            CurrentLogoUrl = s.LogoUrl
        });
    }

    [HttpPost]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Brand(BrandSettingsForm form, CancellationToken ct)
    {
        var raw = await settings.GetRawAsync(ct);
        var currentLogo = raw.GetValueOrDefault(SettingKeys.LogoImage);
        form.CurrentLogoUrl = string.IsNullOrEmpty(currentLogo) ? null : media.GetUrl(MediaFolder.Branding, currentLogo);

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var logo = currentLogo ?? string.Empty;
        string? obsoleteLogo = null;
        if (form.Logo is { Length: > 0 })
        {
            var limits = (await settings.GetAsync(ct)).Media;
            var (fileName, error) = await media.SaveImageAsync(form.Logo, MediaFolder.Branding, limits.MaxFileSizeKb, ct);
            if (error is not null)
            {
                ModelState.AddModelError(nameof(form.Logo), error);
                return View(form);
            }

            obsoleteLogo = currentLogo;
            logo = fileName!;
        }
        else if (form.RemoveLogo)
        {
            obsoleteLogo = currentLogo;
            logo = string.Empty;
        }

        await settings.SaveAsync(new Dictionary<string, string>
        {
            [SettingKeys.AppName] = form.AppName.Trim(),
            [SettingKeys.CompanyName] = form.CompanyName?.Trim() ?? string.Empty,
            [SettingKeys.PrimaryColor] = form.PrimaryColor.ToUpperInvariant(),
            [SettingKeys.LogoImage] = logo
        }, CurrentUserId, ct);

        media.Delete(MediaFolder.Branding, obsoleteLogo);
        return Saved("Marca");
    }

    // ---------- Portal ----------

    [HttpGet]
    public async Task<IActionResult> Portal(CancellationToken ct)
    {
        var p = (await settings.GetAsync(ct)).Portal;
        return View(new PortalSettingsForm
        {
            SearchPlaceholder = p.SearchPlaceholder,
            ShowFilters = p.ShowFilters,
            ShowAppCount = p.ShowAppCount,
            ShowQuickAccess = p.ShowQuickAccess,
            OpenInNewTab = p.OpenInNewTab,
            RecentMax = p.RecentMax,
            RecentDays = p.RecentDays,
            PinnedDays = p.PinnedDays,
            SearchDelayMs = p.SearchDelayMs
        });
    }

    [HttpPost]
    public async Task<IActionResult> Portal(PortalSettingsForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        await settings.SaveAsync(new Dictionary<string, string>
        {
            [SettingKeys.SearchPlaceholder] = form.SearchPlaceholder.Trim(),
            [SettingKeys.ShowFilters] = Flag(form.ShowFilters),
            [SettingKeys.ShowAppCount] = Flag(form.ShowAppCount),
            [SettingKeys.ShowQuickAccess] = Flag(form.ShowQuickAccess),
            [SettingKeys.OpenInNewTab] = Flag(form.OpenInNewTab),
            [SettingKeys.RecentMax] = Number(form.RecentMax),
            [SettingKeys.RecentDays] = Number(form.RecentDays),
            [SettingKeys.PinnedDays] = Number(form.PinnedDays),
            [SettingKeys.SearchDelayMs] = Number(form.SearchDelayMs)
        }, CurrentUserId, ct);

        return Saved("Portal");
    }

    // ---------- Avisos ----------

    [HttpGet]
    public async Task<IActionResult> Notices(CancellationToken ct)
    {
        var n = (await settings.GetAsync(ct)).Notices;
        return View(new NoticeSettingsForm { Label = n.Label, Color = n.Color, SpeedSeconds = n.SpeedSeconds });
    }

    [HttpPost]
    public async Task<IActionResult> Notices(NoticeSettingsForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        await settings.SaveAsync(new Dictionary<string, string>
        {
            [SettingKeys.NoticesLabel] = form.Label.Trim(),
            [SettingKeys.NoticesColor] = form.Color.ToUpperInvariant(),
            [SettingKeys.NoticesSpeedSeconds] = Number(form.SpeedSeconds)
        }, CurrentUserId, ct);

        return Saved("Avisos");
    }

    // ---------- Seguridad ----------

    [HttpGet]
    public async Task<IActionResult> Security(CancellationToken ct)
    {
        var sec = (await settings.GetAsync(ct)).Security;
        return View(new SecuritySettingsForm
        {
            MaxFailedAttempts = sec.MaxFailedAttempts,
            LockoutMinutes = sec.LockoutMinutes,
            SessionMinutes = sec.SessionMinutes,
            PasswordMinLength = sec.PasswordMinLength
        });
    }

    [HttpPost]
    public async Task<IActionResult> Security(SecuritySettingsForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        await settings.SaveAsync(new Dictionary<string, string>
        {
            [SettingKeys.MaxFailedAttempts] = Number(form.MaxFailedAttempts),
            [SettingKeys.LockoutMinutes] = Number(form.LockoutMinutes),
            [SettingKeys.SessionMinutes] = Number(form.SessionMinutes),
            [SettingKeys.PasswordMinLength] = Number(form.PasswordMinLength)
        }, CurrentUserId, ct);

        return Saved("Seguridad");
    }

    // ---------- Imágenes ----------

    [HttpGet]
    public async Task<IActionResult> Media(CancellationToken ct)
    {
        var m = (await settings.GetAsync(ct)).Media;
        return View(new MediaSettingsForm { MaxFileSizeKb = m.MaxFileSizeKb, CarouselMaxImages = m.CarouselMaxImages });
    }

    [HttpPost]
    public async Task<IActionResult> Media(MediaSettingsForm form, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        await settings.SaveAsync(new Dictionary<string, string>
        {
            [SettingKeys.MediaMaxSizeKb] = Number(form.MaxFileSizeKb),
            [SettingKeys.CarouselMaxImages] = Number(form.CarouselMaxImages)
        }, CurrentUserId, ct);

        return Saved("Imágenes");
    }

    // ---------- Regional ----------

    [HttpGet]
    public async Task<IActionResult> Regional(CancellationToken ct)
    {
        var raw = await settings.GetRawAsync(ct);
        var s = await settings.GetAsync(ct);
        return View(new RegionalSettingsForm
        {
            TimeZone = raw.GetValueOrDefault(SettingKeys.TimeZone) ?? s.TimeZone.Id,
            LinkValidationRegex = s.LinkValidationRegex
        });
    }

    [HttpPost]
    public async Task<IActionResult> Regional(RegionalSettingsForm form, CancellationToken ct)
    {
        if (!IsValidRegex(form.LinkValidationRegex))
        {
            ModelState.AddModelError(nameof(form.LinkValidationRegex), "La expresión regular no es válida.");
        }

        if (!string.IsNullOrWhiteSpace(form.TimeZone) && !TimeZoneInfo.TryFindSystemTimeZoneById(form.TimeZone.Trim(), out _))
        {
            ModelState.AddModelError(nameof(form.TimeZone), "Zona horaria no reconocida en este servidor.");
        }

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        await settings.SaveAsync(new Dictionary<string, string>
        {
            [SettingKeys.TimeZone] = form.TimeZone.Trim(),
            [SettingKeys.LinkValidationRegex] = form.LinkValidationRegex.Trim()
        }, CurrentUserId, ct);

        return Saved("Regional");
    }

    private RedirectToActionResult Saved(string section)
    {
        Notify($"Configuración de {section} guardada.");
        return RedirectToAction(nameof(Index));
    }

    private static string Flag(bool value) => value ? "true" : "false";

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Or(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static bool IsValidRegex(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return false;
        }

        try
        {
            _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
