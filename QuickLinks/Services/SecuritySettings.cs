namespace QuickLinks.Services;

/// <summary>Reglas de acceso a la administración.</summary>
public sealed record SecuritySettings(int MaxFailedAttempts, int LockoutMinutes, int SessionMinutes, int PasswordMinLength);
