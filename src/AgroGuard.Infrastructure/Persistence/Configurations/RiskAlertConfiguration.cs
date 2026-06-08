using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroGuard.Infrastructure.Persistence.Configurations;

internal sealed class RiskAlertConfiguration : IEntityTypeConfiguration<RiskAlert>
{
    public void Configure(EntityTypeBuilder<RiskAlert> builder)
    {
        builder.ToTable("AG_RISK_ALERTS");
        builder.HasKey(alert => alert.Id);

        builder.Property(alert => alert.Id).HasColumnType("RAW(16)");
        builder.Property(alert => alert.FieldId).HasColumnType("RAW(16)").IsRequired();
        builder.Property(alert => alert.SatelliteReadingId).HasColumnType("RAW(16)");
        builder.Property(alert => alert.Type).HasConversion<int>().IsRequired();
        builder.Property(alert => alert.Level).HasConversion<int>().IsRequired();
        builder.Property(alert => alert.Score).HasColumnType("NUMBER(5,2)").IsRequired();
        builder.Property(alert => alert.Title).HasMaxLength(140).IsRequired();
        builder.Property(alert => alert.Description).HasMaxLength(500).IsRequired();
        builder.Property(alert => alert.Recommendation).HasMaxLength(500).IsRequired();
        builder.Property(alert => alert.Status).HasConversion<int>().IsRequired();
        builder.Property(alert => alert.CreatedAt).HasColumnType("TIMESTAMP").IsRequired();
        builder.Property(alert => alert.ResolvedAt).HasColumnType("TIMESTAMP");

        builder.HasIndex(alert => new { alert.FieldId, alert.Status, alert.Level });

        builder.HasOne(alert => alert.SatelliteReading)
            .WithMany()
            .HasForeignKey(alert => alert.SatelliteReadingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
