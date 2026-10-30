using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("CT_Applications");
        builder.HasKey(e => e.Id).HasName("PK_CT_Applications");

        builder.Property(e => e.Id).HasColumnName("PKApplication");
        builder.Property(e => e.Name).HasColumnName("ApplicationName").HasMaxLength(100);
        builder.Property(e => e.Version).HasColumnName("ApplicationVersion").HasMaxLength(20);
        builder.Property(e => e.Sme).HasColumnName("ApplicationSME").HasMaxLength(200);
        builder.Property(e => e.UserManual).HasColumnName("ApplicationUserManual").HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnName("ApplicationDescription").HasMaxLength(300);
        builder.Property(e => e.Link).HasColumnName("ApplicationLink").HasMaxLength(500);
        builder.Property(e => e.Image).HasColumnName("ApplicationImage").HasMaxLength(200);

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UX_CT_Applications_ApplicationName");

        builder.ConfigureAudit("FK_CT_Applications_SC_Users");
    }
}
