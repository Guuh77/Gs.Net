using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroGuard.Infrastructure.Persistence.Configurations;

internal sealed class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.ToTable("AG_FIELDS");
        builder.HasKey(field => field.Id);

        builder.Property(field => field.Id).HasColumnType("RAW(16)");
        builder.Property(field => field.FarmId).HasColumnType("RAW(16)").IsRequired();
        builder.Property(field => field.CropId).HasColumnType("RAW(16)").IsRequired();
        builder.Property(field => field.Name).HasMaxLength(120).IsRequired();
        builder.Property(field => field.AreaHectares).HasColumnType("NUMBER(12,2)").IsRequired();
        builder.Property(field => field.Latitude).HasColumnType("NUMBER(9,6)").IsRequired();
        builder.Property(field => field.Longitude).HasColumnType("NUMBER(9,6)").IsRequired();
        builder.Property(field => field.SoilType).HasMaxLength(80).IsRequired();
        builder.Property(field => field.PlantedAt).HasColumnType("TIMESTAMP").IsRequired();
        builder.Property(field => field.ExpectedHarvestAt).HasColumnType("TIMESTAMP").IsRequired();
        builder.Property(field => field.CreatedAt).HasColumnType("TIMESTAMP").IsRequired();

        builder.HasIndex(field => new { field.FarmId, field.Name }).IsUnique();

        builder.HasMany(field => field.SatelliteReadings)
            .WithOne(reading => reading.Field)
            .HasForeignKey(reading => reading.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(field => field.RiskAlerts)
            .WithOne(alert => alert.Field)
            .HasForeignKey(alert => alert.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(field => field.SatelliteReadings).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(field => field.RiskAlerts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
