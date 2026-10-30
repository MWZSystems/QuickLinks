using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class ApplicationGroupConfiguration : IEntityTypeConfiguration<ApplicationGroup>
{
    public void Configure(EntityTypeBuilder<ApplicationGroup> builder)
    {
        builder.ToTable("SY_ApplicationGroups");
        builder.HasKey(e => e.Id).HasName("PK_SY_ApplicationGroups");

        builder.Property(e => e.Id).HasColumnName("PKApplicationGroup");
        builder.Property(e => e.ApplicationId).HasColumnName("FKApplication");
        builder.Property(e => e.GroupId).HasColumnName("FKGroup");

        builder.HasOne(e => e.Application)
            .WithMany(a => a.Groups)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_SY_ApplicationGroups_CT_Applications");

        builder.HasOne(e => e.Group)
            .WithMany(g => g.Applications)
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_SY_ApplicationGroups_CT_Groups");

        builder.HasIndex(e => new { e.ApplicationId, e.GroupId })
            .IsUnique()
            .HasDatabaseName("UX_SY_ApplicationGroups_FKApplication_FKGroup");
        builder.HasIndex(e => e.GroupId).HasDatabaseName("IX_SY_ApplicationGroups_FKGroup");

        builder.ConfigureAudit("FK_SY_ApplicationGroups_SC_Users");
    }
}
