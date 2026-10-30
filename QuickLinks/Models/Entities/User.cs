namespace QuickLinks.Models.Entities;

public class User : AuditedEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    /// <summary>Hash PBKDF2 generado por <see cref="Microsoft.AspNetCore.Identity.PasswordHasher{TUser}"/>.</summary>
    public string? PasswordHash { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public DateTime? LastLogin { get; set; }
}
