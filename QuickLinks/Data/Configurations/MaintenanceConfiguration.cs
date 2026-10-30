using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class MaintenanceConfiguration : IEntityTypeConfiguration<Maintenance>
{
    public void Configure(EntityTypeBuilder<Maintenance> builder)
    {
        builder.ToTable("CT_Maintenance");
        builder.HasKey(e => e.Id).HasName("PK_CT_Maintenance");

        builder.Property(e => e.Id).HasColumnName("PkMaintenance");
        builder.Property(e => e.ApplicationId).HasColumnName("FkApplication");
        builder.Property(e => e.CustomMessage).HasMaxLength(500);
        builder.Property(e => e.StartDate).HasColumnType("datetime2(0)");
        builder.Property(e => e.EndDate).HasColumnType("datetime2(0)");

        builder.HasOne(e => e.Application)
            .WithMany(a => a.Maintenances)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CT_Maintenance_CT_Applications");

        builder.HasIndex(e => e.ApplicationId).HasDatabaseName("IX_CT_Maintenance_FkApplication");

        builder.ConfigureAudit("FK_CT_Maintenance_SC_Users");
    }
}
