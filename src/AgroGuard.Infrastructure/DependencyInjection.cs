using AgroGuard.Application.Abstractions;
using AgroGuard.Infrastructure.Authentication;
using AgroGuard.Infrastructure.Nasa;
using AgroGuard.Infrastructure.Persistence;
using AgroGuard.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Oracle")
            ?? throw new InvalidOperationException("Connection string 'Oracle' was not configured.");

        services.AddDbContext<AgroGuardDbContext>(options =>
            options.UseOracle(
                connectionString,
                oracle => oracle.MigrationsAssembly(typeof(AgroGuardDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AgroGuardDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFarmRepository, FarmRepository>();
        services.AddScoped<ICropRepository, CropRepository>();
        services.AddScoped<IFieldRepository, FieldRepository>();
        services.AddScoped<ISatelliteReadingRepository, SatelliteReadingRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHttpClient<INasaPowerClient, NasaPowerClient>(client =>
        {
            client.BaseAddress = new Uri("https://power.larc.nasa.gov/");
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        services.AddHttpClient<INasaEventClient, NasaEonetClient>(client =>
        {
            client.BaseAddress = new Uri("https://eonet.gsfc.nasa.gov/api/v3/");
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        return services;
    }
}
