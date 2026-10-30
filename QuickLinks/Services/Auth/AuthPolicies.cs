namespace QuickLinks.Services.Auth;

public static class AuthPolicies
{
    /// <summary>Política de los catálogos administrativos: usuario activo con rol Administrador.</summary>
    public const string Administrator = "Administrator";

    public const string RoleIdClaim = "role_id";
}
