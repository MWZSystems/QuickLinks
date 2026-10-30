using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;

namespace QuickLinks.Data.Configurations;

internal sealed class CarouselImageConfiguration : IEntityTypeConfiguration<CarouselImage>
{
    public void Configure(EntityTypeBuilder<CarouselImage> builder)
    {
        builder.ToTable("CT_Carousel");
        builder.HasKey(e => e.Id).HasName("PK_CT_Carousel");

        builder.Property(e => e.Id).HasColumnName("PKCarousel");
        builder.Property(e => e.ApplicationId).HasColumnName("FKApplication");
        builder.Property(e => e.ImageName).HasMaxLength(100);

        builder.HasOne(e => e.Application)
            .WithMany(a => a.CarouselImages)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CT_Carousel_CT_Applications");

        builder.HasIndex(e => e.ApplicationId).HasDatabaseName("IX_CT_Carousel_FKApplication");

        builder.ConfigureAudit("FK_CT_Carousel_SC_Users");
    }
}
