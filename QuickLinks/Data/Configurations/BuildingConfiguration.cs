using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("CT_Buildings");
        builder.HasKey(e => e.Id).HasName("PK_CT_Buildings");

        builder.Property(e => e.Id).HasColumnName("PKBuilding");
        builder.Property(e => e.SiteId).HasColumnName("FKSite");
        builder.Property(e => e.Name).HasMaxLength(50);

        builder.HasOne(e => e.Site)
            .WithMany(s => s.Buildings)
            .HasForeignKey(e => e.SiteId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CT_Buildings_CT_Sites");

        builder.HasIndex(e => new { e.SiteId, e.Name })
            .IsUnique()
            .HasDatabaseName("UX_CT_Buildings_FKSite_Name");

        builder.ConfigureAudit("FK_CT_Buildings_SC_Users");
    }
}
