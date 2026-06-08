using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroGuard.Infrastructure.Persistence.Configurations;

internal sealed class SatelliteReadingConfiguration : IEntityTypeConfiguration<SatelliteReading>
{
    public void Configure(EntityTypeBuilder<SatelliteReading> builder)
    {
        builder.ToTable("AG_SATELLITE_READINGS");
        builder.HasKey(reading => reading.Id);

        builder.Property(reading => reading.Id).HasColumnType("RAW(16)");
        builder.Property(reading => reading.FieldId).HasColumnType("RAW(16)").IsRequired();
        builder.Property(reading => reading.CapturedAt).HasColumnType("TIMESTAMP").IsRequired();
        builder.Property(reading => reading.Source).HasMaxLength(120).IsRequired();
        builder.Property(reading => reading.Ndvi).HasColumnType("NUMBER(4,3)").IsRequired();
        builder.Property(reading => reading.SoilMoisturePercent).HasColumnType("NUMBER(5,2)").IsRequired();
        builder.Property(reading => reading.SurfaceTemperatureCelsius).HasColumnType("NUMBER(5,2)").IsRequired();
        builder.Property(reading => reading.RainfallMillimeters).HasColumnType("NUMBER(7,2)").IsRequired();
        builder.Property(reading => reading.CloudCoveragePercent).HasColumnType("NUMBER(5,2)").IsRequired();
        builder.Property(reading => reading.CreatedAt).HasColumnType("TIMESTAMP").IsRequired();

        builder.HasIndex(reading => new { reading.FieldId, reading.CapturedAt });
    }
}
