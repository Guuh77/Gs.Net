using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgroGuard.Infrastructure.Persistence;

public sealed class AgroGuardDbContextFactory : IDesignTimeDbContextFactory<AgroGuardDbContext>
{
    public AgroGuardDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AgroGuardDbContext>()
            .UseOracle(
                "User Id=AGROGUARD;Password=AgroGuard123;Data Source=localhost:1521/FREEPDB1;",
                oracle => oracle.MigrationsAssembly(typeof(AgroGuardDbContext).Assembly.FullName))
            .Options;

        return new AgroGuardDbContext(options);
    }
}
