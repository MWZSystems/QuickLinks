using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using QuickLinks.Models.ViewModels.Admin;
using QuickLinks.Services;

namespace QuickLinks.Controllers.Admin;

/// <summary>Marca y parámetros del cliente guardados en CT_Settings.</summary>
public class SettingsController(ISettingsService settings, IMediaStorage media) : AdminController
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var raw = await settings.GetRawAsync(ct);
        var portal = await settings.GetAsync(ct);
        return View(new SettingsForm
        {
            AppName = portal.AppName,
            CompanyName = raw.GetValueOrDefault(SettingKeys.CompanyName),
            PrimaryColor = portal.PrimaryColor,
            LinkValidationRegex = portal.LinkValidationRegex,
            TimeZone = raw.GetValueOrDefault(SettingKeys.TimeZone) ?? portal.TimeZone.Id,
            CurrentLogoUrl = portal.LogoUrl
        });
    }

    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Index(SettingsForm form, CancellationToken ct)
    {
        var raw = await settings.GetRawAsync(ct);
        var currentLogo = raw.GetValueOrDefault(SettingKeys.LogoImage);
        form.CurrentLogoUrl = string.IsNullOrEmpty(currentLogo) ? null : media.GetUrl(MediaFolder.Branding, currentLogo);

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

        var logo = currentLogo ?? string.Empty;
        string? obsoleteLogo = null;
        if (form.Logo is { Length: > 0 })
        {
            var (fileName, error) = await media.SaveImageAsync(form.Logo, MediaFolder.Branding, ct);
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
            [SettingKeys.LinkValidationRegex] = form.LinkValidationRegex.Trim(),
            [SettingKeys.TimeZone] = form.TimeZone.Trim(),
            [SettingKeys.LogoImage] = logo
        }, CurrentUserId, ct);

        media.Delete(MediaFolder.Branding, obsoleteLogo);
        Notify("Configuración guardada.");
        return RedirectToAction(nameof(Index));
    }

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
