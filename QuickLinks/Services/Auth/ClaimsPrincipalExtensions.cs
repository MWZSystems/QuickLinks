using System.Security.Claims;

namespace QuickLinks.Services.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>PKUser del usuario autenticado.</summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("El usuario actual no tiene identificador.");
    }
}
