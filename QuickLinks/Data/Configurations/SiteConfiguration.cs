using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.ToTable("CT_Sites");
        builder.HasKey(e => e.Id).HasName("PK_CT_Sites");

        builder.Property(e => e.Id).HasColumnName("PKSite");
        builder.Property(e => e.Name).HasMaxLength(50);
        builder.Property(e => e.Address).HasMaxLength(200);
        builder.Property(e => e.CreatedByUserId).HasColumnName("FKUserCreator");
        builder.Property(e => e.CreationDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UX_CT_Sites_Name");

        builder.ConfigureAudit(updaterForeignKeyName: null);
    }
}
