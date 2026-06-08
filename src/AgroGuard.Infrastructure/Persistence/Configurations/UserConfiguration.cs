using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgroGuard.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("AG_USERS");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).HasColumnType("RAW(16)");
        builder.Property(user => user.Name).HasMaxLength(120).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(180).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(user => user.Role).HasConversion<int>().IsRequired();
        builder.Property(user => user.CreatedAt).HasColumnType("TIMESTAMP").IsRequired();

        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasMany(user => user.Farms)
            .WithOne(farm => farm.Owner)
            .HasForeignKey(farm => farm.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.Farms).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
