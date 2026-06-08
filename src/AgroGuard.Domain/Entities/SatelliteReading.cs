using AgroGuard.Domain.Common;

namespace AgroGuard.Domain.Entities;

public sealed class SatelliteReading : Entity
{
    private SatelliteReading()
    {
    }

    public SatelliteReading(
        Guid fieldId,
        DateTime capturedAt,
        string source,
        decimal ndvi,
        decimal soilMoisturePercent,
        decimal surfaceTemperatureCelsius,
        decimal rainfallMillimeters,
        decimal cloudCoveragePercent)
    {
        FieldId = fieldId;
        CapturedAt = capturedAt;
        Source = GuardRequired(source, nameof(source));
        Ndvi = GuardRange(ndvi, 0m, 1m, nameof(ndvi));
        SoilMoisturePercent = GuardRange(soilMoisturePercent, 0m, 100m, nameof(soilMoisturePercent));
        SurfaceTemperatureCelsius = GuardRange(surfaceTemperatureCelsius, -20m, 70m, nameof(surfaceTemperatureCelsius));
        RainfallMillimeters = GuardRange(rainfallMillimeters, 0m, 1000m, nameof(rainfallMillimeters));
        CloudCoveragePercent = GuardRange(cloudCoveragePercent, 0m, 100m, nameof(cloudCoveragePercent));
        CreatedAt = DateTime.UtcNow;
    }

    public Guid FieldId { get; private set; }
    public DateTime CapturedAt { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public decimal Ndvi { get; private set; }
    public decimal SoilMoisturePercent { get; private set; }
    public decimal SurfaceTemperatureCelsius { get; private set; }
    public decimal RainfallMillimeters { get; private set; }
    public decimal CloudCoveragePercent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Field Field { get; private set; } = null!;

    private static string GuardRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static decimal GuardRange(decimal value, decimal min, decimal max, string parameterName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be between {min} and {max}.");
        }

        return value;
    }
}
