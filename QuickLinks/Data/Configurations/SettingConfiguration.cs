using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickLinks.Models.Entities;
using QuickLinks.Services;

namespace QuickLinks.Data.Configurations;

internal sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("CT_Settings");
        builder.HasKey(e => e.Id).HasName("PK_CT_Settings");

        builder.Property(e => e.Id).HasColumnName("PKSetting");
        builder.Property(e => e.Name).HasColumnName("SettingName").HasMaxLength(50);
        builder.Property(e => e.Value).HasColumnName("SettingValue").HasMaxLength(100);
        builder.Property(e => e.SortOrder).HasColumnName("SettingOrder");

        builder.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UX_CT_Settings_SettingName");

        builder.ConfigureAudit(updaterForeignKeyName: null);

        builder.HasData(SettingKeys.InitialSeed.Select((d, index) => new Setting
        {
            Id = index + 1,
            Name = d.Key,
            Value = d.Value,
            SortOrder = index + 1,
            UpdatedByUserId = SeedData.AdminUserId,
            LastUpdated = SeedData.Timestamp,
            Available = true
        }));
    }
}
