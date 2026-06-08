using AgroGuard.Domain.Common;

namespace AgroGuard.Domain.Entities;

public sealed class Field : Entity
{
    private readonly List<SatelliteReading> _satelliteReadings = [];
    private readonly List<RiskAlert> _riskAlerts = [];

    private Field()
    {
    }

    public Field(
        Guid farmId,
        Guid cropId,
        string name,
        decimal areaHectares,
        decimal latitude,
        decimal longitude,
        string soilType,
        DateTime plantedAt,
        DateTime expectedHarvestAt)
    {
        FarmId = farmId;
        CropId = cropId;
        Name = GuardRequired(name, nameof(name));
        AreaHectares = GuardPositive(areaHectares, nameof(areaHectares));
        Latitude = GuardRange(latitude, -90m, 90m, nameof(latitude));
        Longitude = GuardRange(longitude, -180m, 180m, nameof(longitude));
        SoilType = GuardRequired(soilType, nameof(soilType));
        PlantedAt = plantedAt;
        ExpectedHarvestAt = expectedHarvestAt;
        CreatedAt = DateTime.UtcNow;

        if (expectedHarvestAt <= plantedAt)
        {
            throw new ArgumentException("Expected harvest date must be after planting date.");
        }
    }

    public Field(
        Guid farmId,
        Crop crop,
        string name,
        decimal areaHectares,
        decimal latitude,
        decimal longitude,
        string soilType,
        DateTime plantedAt,
        DateTime expectedHarvestAt)
        : this(farmId, crop.Id, name, areaHectares, latitude, longitude, soilType, plantedAt, expectedHarvestAt)
    {
        Crop = crop;
    }

    public Guid FarmId { get; private set; }
    public Guid CropId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal AreaHectares { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string SoilType { get; private set; } = string.Empty;
    public DateTime PlantedAt { get; private set; }
    public DateTime ExpectedHarvestAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Farm Farm { get; private set; } = null!;
    public Crop Crop { get; private set; } = null!;
    public IReadOnlyCollection<SatelliteReading> SatelliteReadings => _satelliteReadings;
    public IReadOnlyCollection<RiskAlert> RiskAlerts => _riskAlerts;

    private static string GuardRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static decimal GuardPositive(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"{parameterName} must be positive.");
        }

        return value;
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
