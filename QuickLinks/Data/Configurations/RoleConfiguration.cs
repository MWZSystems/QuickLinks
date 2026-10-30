using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("CT_Roles");
        builder.HasKey(e => e.Id).HasName("PK_CT_Roles");

        builder.Property(e => e.Id).HasColumnName("PKRole");
        builder.Property(e => e.Name).HasMaxLength(100);

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UX_CT_Roles_Name");

        // Sin FK de auditoría: SC_Users referencia a CT_Roles y la relación inversa haría circular la carga inicial.
        builder.ConfigureAudit(updaterForeignKeyName: null);

        builder.HasData(new Role
        {
            Id = Role.AdministratorId,
            Name = "Administrador",
            UpdatedByUserId = 1,
            LastUpdated = SeedData.Timestamp,
            Available = true
        });
    }
}
