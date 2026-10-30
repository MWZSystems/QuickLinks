using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("CT_Categories");
        builder.HasKey(e => e.Id).HasName("PK_CT_Categories");

        builder.Property(e => e.Id).HasColumnName("PKCategory");
        builder.Property(e => e.Name).HasMaxLength(100);
        builder.Property(e => e.Color)
            .HasMaxLength(7)
            .IsUnicode(false)
            .HasDefaultValue(Category.DefaultColor);

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UX_CT_Categories_Name");

        builder.ConfigureAudit("FK_CT_Categories_SC_Users");
    }
}
