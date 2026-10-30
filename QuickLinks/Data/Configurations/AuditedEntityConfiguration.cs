using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

/// <summary>
/// Mapeo común de las columnas de auditoría hacia la nomenclatura del esquema (FKUserUpdater, LastUpdated, Available).
/// </summary>
internal static class AuditedEntityConfiguration
{
    /// <param name="updaterForeignKeyName">
    /// Nombre de la FK hacia SC_Users. Nulo cuando la tabla no tiene FK física de auditoría
    /// (SC_Users, CT_Roles, CT_Settings y CT_Sites), igual que en el esquema original.
    /// </param>
    public static void ConfigureAudit<T>(this EntityTypeBuilder<T> builder, string? updaterForeignKeyName)
        where T : AuditedEntity
    {
        builder.Property(e => e.UpdatedByUserId).HasColumnName("FKUserUpdater");
        builder.Property(e => e.LastUpdated)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(e => e.Available).HasDefaultValue(true);

        if (updaterForeignKeyName is not null)
        {
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UpdatedByUserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(updaterForeignKeyName);
        }
    }
}
