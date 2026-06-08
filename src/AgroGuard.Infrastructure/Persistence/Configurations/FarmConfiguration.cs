using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroGuard.Infrastructure.Persistence.Configurations;

internal sealed class FarmConfiguration : IEntityTypeConfiguration<Farm>
{
    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.ToTable("AG_FARMS");
        builder.HasKey(farm => farm.Id);

        builder.Property(farm => farm.Id).HasColumnType("RAW(16)");
        builder.Property(farm => farm.OwnerId).HasColumnType("RAW(16)").IsRequired();
        builder.Property(farm => farm.Name).HasMaxLength(120).IsRequired();
        builder.Property(farm => farm.City).HasMaxLength(80).IsRequired();
        builder.Property(farm => farm.State).HasMaxLength(2).IsRequired();
        builder.Property(farm => farm.Latitude).HasColumnType("NUMBER(9,6)").IsRequired();
        builder.Property(farm => farm.Longitude).HasColumnType("NUMBER(9,6)").IsRequired();
        builder.Property(farm => farm.TotalAreaHectares).HasColumnType("NUMBER(12,2)").IsRequired();
        builder.Property(farm => farm.CreatedAt).HasColumnType("TIMESTAMP").IsRequired();

        builder.HasIndex(farm => new { farm.OwnerId, farm.Name }).IsUnique();

        builder.HasMany(farm => farm.Fields)
            .WithOne(field => field.Farm)
            .HasForeignKey(field => field.FarmId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(farm => farm.Fields).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
