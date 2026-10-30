using Microsoft.EntityFrameworkCore;
using QuickLinks.Services.Auth;

namespace QuickLinks.Data;

/// <summary>
/// Asigna la contraseña del administrador inicial a partir de <c>Seed:AdminPassword</c>
/// cuando la cuenta aún no tiene una. No aplica migraciones.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var admin = await db.Users.FirstOrDefaultAsync(u => u.Id == SeedData.AdminUserId);
        if (admin is null || !string.IsNullOrEmpty(admin.PasswordHash))
        {
            return;
        }

        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "El usuario {UserName} no tiene contraseña. Define Seed:AdminPassword para asignarla en el siguiente arranque",
                admin.UserName);
            return;
        }

        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
        admin.PasswordHash = auth.HashPassword(password);
        await db.SaveChangesAsync();
        logger.LogInformation("Contraseña inicial asignada al usuario {UserName}", admin.UserName);
    }
}
