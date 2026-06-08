using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroGuard.Infrastructure.Persistence.Configurations;

internal sealed class CropConfiguration : IEntityTypeConfiguration<Crop>
{
    public void Configure(EntityTypeBuilder<Crop> builder)
    {
        builder.ToTable("AG_CROPS");
        builder.HasKey(crop => crop.Id);

        builder.Property(crop => crop.Id).HasColumnType("RAW(16)");
        builder.Property(crop => crop.Name).HasMaxLength(80).IsRequired();
        builder.Property(crop => crop.ScientificName).HasMaxLength(120).IsRequired();
        builder.Property(crop => crop.IdealNdvi).HasColumnType("NUMBER(4,3)").IsRequired();
        builder.Property(crop => crop.WaterDemandIndex).HasColumnType("NUMBER(4,3)").IsRequired();

        builder.HasIndex(crop => crop.Name).IsUnique();

        builder.HasMany(crop => crop.Fields)
            .WithOne(field => field.Crop)
            .HasForeignKey(field => field.CropId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(crop => crop.Fields).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(
            new
            {
                Id = SeedData.SoybeanId,
                Name = "Soybean",
                ScientificName = "Glycine max",
                IdealNdvi = 0.82m,
                WaterDemandIndex = 0.78m
            },
            new
            {
                Id = SeedData.CornId,
                Name = "Corn",
                ScientificName = "Zea mays",
                IdealNdvi = 0.80m,
                WaterDemandIndex = 0.84m
            },
            new
            {
                Id = SeedData.CoffeeId,
                Name = "Coffee",
                ScientificName = "Coffea arabica",
                IdealNdvi = 0.74m,
                WaterDemandIndex = 0.66m
            },
            new
            {
                Id = SeedData.SugarcaneId,
                Name = "Sugarcane",
                ScientificName = "Saccharum officinarum",
                IdealNdvi = 0.86m,
                WaterDemandIndex = 0.88m
            });
    }
}
