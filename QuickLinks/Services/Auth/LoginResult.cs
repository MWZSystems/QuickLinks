using System.Security.Claims;

namespace QuickLinks.Services.Auth;

public sealed record LoginResult(ClaimsPrincipal? Principal, string? Error)
{
    public bool Succeeded => Principal is not null;

    public static LoginResult Fail(string error) => new(null, error);
}
