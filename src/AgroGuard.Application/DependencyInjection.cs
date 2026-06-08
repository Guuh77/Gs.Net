using AgroGuard.Application.Alerts;
using AgroGuard.Application.Auth;
using AgroGuard.Application.Crops;
using AgroGuard.Application.Dashboard;
using AgroGuard.Application.Farms;
using AgroGuard.Application.Fields;
using AgroGuard.Application.Nasa;
using AgroGuard.Application.Readings;
using AgroGuard.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AgroGuard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<RiskAssessmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICropService, CropService>();
        services.AddScoped<IFarmService, FarmService>();
        services.AddScoped<IFieldService, FieldService>();
        services.AddScoped<ISatelliteMonitoringService, SatelliteMonitoringService>();
        services.AddScoped<INasaMonitoringService, NasaMonitoringService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
