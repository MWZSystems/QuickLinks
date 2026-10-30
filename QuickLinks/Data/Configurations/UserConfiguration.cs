using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("SC_Users");
        builder.HasKey(e => e.Id).HasName("PK_SC_Users");

        builder.Property(e => e.Id).HasColumnName("PKUser");
        builder.Property(e => e.Name).HasMaxLength(50);
        builder.Property(e => e.UserName).HasMaxLength(50);
        builder.Property(e => e.RoleId).HasColumnName("FKRole");
        builder.Property(e => e.PasswordHash).HasMaxLength(256);
        builder.Property(e => e.LockoutEnd).HasColumnType("datetime2(0)");
        builder.Property(e => e.LastLogin).HasColumnType("datetime2(0)");

        builder.HasOne(e => e.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_SC_Users_CT_Roles");

        builder.HasIndex(e => e.UserName).IsUnique().HasDatabaseName("UX_SC_Users_UserName");
        builder.HasIndex(e => e.RoleId).HasDatabaseName("IX_SC_Users_FKRole");

        builder.ConfigureAudit(updaterForeignKeyName: null);

        // Administrador inicial sin contraseña; DbInitializer la asigna desde configuración en el primer arranque.
        builder.HasData(new User
        {
            Id = SeedData.AdminUserId,
            Name = "Administrador",
            UserName = "admin",
            RoleId = Role.AdministratorId,
            UpdatedByUserId = SeedData.AdminUserId,
            LastUpdated = SeedData.Timestamp,
            Available = true
        });
    }
}
