namespace QuickLinks.Services.Auth;

public interface IAuthService
{
    Task<LoginResult> SignInAsync(string userName, string password, CancellationToken ct = default);

    /// <summary>Indica si el usuario sigue activo y con rol de administrador.</summary>
    Task<bool> IsStillAuthorizedAsync(int userId, CancellationToken ct = default);

    string HashPassword(string password);
}
