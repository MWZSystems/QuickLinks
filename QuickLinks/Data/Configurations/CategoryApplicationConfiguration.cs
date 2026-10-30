using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class CategoryApplicationConfiguration : IEntityTypeConfiguration<CategoryApplication>
{
    public void Configure(EntityTypeBuilder<CategoryApplication> builder)
    {
        builder.ToTable("CT_CategoryApplications");
        builder.HasKey(e => e.Id).HasName("PK_CT_CategoryApplications");

        builder.Property(e => e.Id).HasColumnName("PKCategoryApplication");
        builder.Property(e => e.CategoryId).HasColumnName("FKCategory");
        builder.Property(e => e.ApplicationId).HasColumnName("FKApplication");

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Applications)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CT_CategoryApplications_CT_Categories");

        builder.HasOne(e => e.Application)
            .WithMany(a => a.Categories)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CT_CategoryApplications_CT_Applications");

        builder.HasIndex(e => new { e.ApplicationId, e.CategoryId })
            .IsUnique()
            .HasDatabaseName("UX_CT_CategoryApplications_FKApplication_FKCategory");
        builder.HasIndex(e => e.CategoryId).HasDatabaseName("IX_CT_CategoryApplications_FKCategory");

        builder.ConfigureAudit("FK_CT_CategoryApplications_SC_Users");
    }
}
