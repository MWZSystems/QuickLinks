using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class AdviceConfiguration : IEntityTypeConfiguration<Advice>
{
    public void Configure(EntityTypeBuilder<Advice> builder)
    {
        builder.ToTable("CT_Advices");
        builder.HasKey(e => e.Id).HasName("PK_CT_Advices");

        builder.Property(e => e.Id).HasColumnName("PkAdvice");
        builder.Property(e => e.Title).HasMaxLength(50);
        builder.Property(e => e.Content).HasMaxLength(500);
        builder.Property(e => e.StartDate).HasColumnType("datetime2(0)");
        builder.Property(e => e.EndDate).HasColumnType("datetime2(0)");

        builder.HasIndex(e => e.Title).IsUnique().HasDatabaseName("UX_CT_Advices_Title");

        builder.ConfigureAudit("FK_CT_Advices_SC_Users");
    }
}
