using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickLinks.Models.ViewModels;
using QuickLinks.Services;
using QuickLinks.Services.Auth;

namespace QuickLinks.Controllers;

[AllowAnonymous]
public class AccountController(IAuthService auth, ISettingsService settings) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await auth.SignInAsync(model.UserName, model.Password, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.Password = string.Empty;
            return View(model);
        }

        // La renovación deslizante conserva este intervalo: la sesión vence tras ese tiempo sin actividad.
        var sessionMinutes = (await settings.GetAsync(ct)).Security.SessionMinutes;
        var now = DateTimeOffset.UtcNow;
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal!, new AuthenticationProperties
        {
            IssuedUtc = now,
            ExpiresUtc = now.AddMinutes(sessionMinutes),
            AllowRefresh = true
        });
        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Applications");
}
