using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class ApplicationLocationConfiguration : IEntityTypeConfiguration<ApplicationLocation>
{
    public void Configure(EntityTypeBuilder<ApplicationLocation> builder)
    {
        builder.ToTable("SY_ApplicationLocation");
        builder.HasKey(e => e.Id).HasName("PK_SY_ApplicationLocation");

        builder.Property(e => e.Id).HasColumnName("PKApplicationSite");
        builder.Property(e => e.ApplicationId).HasColumnName("FKApplication");
        builder.Property(e => e.BuildingId).HasColumnName("FKBuilding");

        builder.HasOne(e => e.Application)
            .WithMany(a => a.Locations)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_SY_ApplicationLocation_CT_Applications");

        builder.HasOne(e => e.Building)
            .WithMany(b => b.Applications)
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_SY_ApplicationLocation_CT_Buildings");

        builder.HasIndex(e => new { e.ApplicationId, e.BuildingId })
            .IsUnique()
            .HasDatabaseName("UX_SY_ApplicationLocation_FKApplication_FKBuilding");
        builder.HasIndex(e => e.BuildingId).HasDatabaseName("IX_SY_ApplicationLocation_FKBuilding");

        builder.ConfigureAudit("FK_SY_ApplicationLocation_SC_Users");
    }
}
