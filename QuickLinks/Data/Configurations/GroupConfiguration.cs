using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("CT_Groups");
        builder.HasKey(e => e.Id).HasName("PK_CT_Groups");

        builder.Property(e => e.Id).HasColumnName("PKGroup");
        builder.Property(e => e.Name).HasMaxLength(50);

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UX_CT_Groups_Name");

        builder.ConfigureAudit("FK_CT_Groups_SC_Users");
    }
}
