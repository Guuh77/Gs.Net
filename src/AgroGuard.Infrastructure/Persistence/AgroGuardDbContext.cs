using AgroGuard.Application.Abstractions;
using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroGuard.Infrastructure.Persistence;

public sealed class AgroGuardDbContext : DbContext, IUnitOfWork
{
    public AgroGuardDbContext(DbContextOptions<AgroGuardDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<SatelliteReading> SatelliteReadings => Set<SatelliteReading>();
    public DbSet<RiskAlert> RiskAlerts => Set<RiskAlert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgroGuardDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
