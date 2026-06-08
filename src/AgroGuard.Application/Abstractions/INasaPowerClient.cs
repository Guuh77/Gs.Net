namespace AgroGuard.Application.Abstractions;

public interface INasaPowerClient
{
    Task<NasaPowerClimateSummary> GetAgroClimateAsync(
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken);

    Task<NasaPowerClimateSeries> GetClimateSeriesAsync(
        decimal latitude,
        decimal longitude,
        int days,
        CancellationToken cancellationToken);
}

public sealed record NasaPowerClimateSummary(
    string Source,
    DateTime StartDate,
    DateTime EndDate,
    decimal AverageTemperatureCelsius,
    decimal MaximumTemperatureCelsius,
    decimal TotalRainfallMillimeters,
    decimal AverageRelativeHumidityPercent,
    decimal RootZoneSoilWetness,
    decimal SurfaceSoilWetness,
    decimal AverageCloudCoveragePercent);

public sealed record NasaPowerClimateSeries(
    NasaPowerClimateSummary Summary,
    IReadOnlyList<NasaPowerDailyClimate> Daily);

public sealed record NasaPowerDailyClimate(
    DateTime Date,
    decimal TemperatureCelsius,
    decimal MaximumTemperatureCelsius,
    decimal MinimumTemperatureCelsius,
    decimal RainfallMillimeters,
    decimal RelativeHumidityPercent,
    decimal SolarRadiationMjPerSquareMeter,
    decimal RootZoneSoilWetness,
    decimal SurfaceSoilWetness,
    decimal CloudCoveragePercent);
