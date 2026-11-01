using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuickLinks.Data;
using QuickLinks.Models.Entities;

namespace QuickLinks.Services.Auth;

/// <summary>
/// Autenticación con cuentas locales de SC_Users: hash PBKDF2 y bloqueo temporal por intentos fallidos.
/// </summary>
public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    ISettingsService settings,
    ILogger<AuthService> logger) : IAuthService
{
    private const string InvalidCredentials = "Usuario o contraseña incorrectos.";

    public async Task<LoginResult> SignInAsync(string userName, string password, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == userName.Trim(), ct);

        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        {
            logger.LogWarning("Inicio de sesión fallido para {UserName}: usuario inexistente o sin contraseña", userName);
            return LoginResult.Fail(InvalidCredentials);
        }

        var now = DateTime.UtcNow;
        if (user.LockoutEnd > now)
        {
            logger.LogWarning("Inicio de sesión bloqueado para {UserId} hasta {LockoutEnd}", user.Id, user.LockoutEnd);
            return LoginResult.Fail("La cuenta está bloqueada temporalmente por intentos fallidos. Intenta más tarde.");
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            var security = (await settings.GetAsync(ct)).Security;
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= security.MaxFailedAttempts)
            {
                user.LockoutEnd = now.AddMinutes(security.LockoutMinutes);
                user.AccessFailedCount = 0;
            }

            await db.SaveChangesAsync(ct);
            logger.LogWarning("Contraseña incorrecta para {UserId}", user.Id);
            return LoginResult.Fail(InvalidCredentials);
        }

        if (!user.Available || user.RoleId != Role.AdministratorId)
        {
            logger.LogWarning("Inicio de sesión rechazado para {UserId}: inactivo o sin rol de administrador", user.Id);
            return LoginResult.Fail("Tu usuario no tiene acceso a la administración.");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastLogin = now;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Inicio de sesión de {UserId}", user.Id);

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(AuthPolicies.RoleIdClaim, user.RoleId.ToString())
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new LoginResult(new ClaimsPrincipal(identity), null);
    }

    public Task<bool> IsStillAuthorizedAsync(int userId, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Id == userId && u.Available && u.RoleId == Role.AdministratorId, ct);

    public string HashPassword(string password) => passwordHasher.HashPassword(null!, password);
}
