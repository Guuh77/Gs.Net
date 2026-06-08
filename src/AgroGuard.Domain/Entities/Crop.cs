using AgroGuard.Domain.Common;

namespace AgroGuard.Domain.Entities;

public sealed class Crop : Entity
{
    private readonly List<Field> _fields = [];

    private Crop()
    {
    }

    public Crop(string name, string scientificName, decimal idealNdvi, decimal waterDemandIndex)
    {
        Name = GuardRequired(name, nameof(name));
        ScientificName = GuardRequired(scientificName, nameof(scientificName));
        IdealNdvi = GuardRange(idealNdvi, 0.1m, 1m, nameof(idealNdvi));
        WaterDemandIndex = GuardRange(waterDemandIndex, 0m, 1m, nameof(waterDemandIndex));
    }

    public string Name { get; private set; } = string.Empty;
    public string ScientificName { get; private set; } = string.Empty;
    public decimal IdealNdvi { get; private set; }
    public decimal WaterDemandIndex { get; private set; }
    public IReadOnlyCollection<Field> Fields => _fields;

    public void Update(string name, string scientificName, decimal idealNdvi, decimal waterDemandIndex)
    {
        Name = GuardRequired(name, nameof(name));
        ScientificName = GuardRequired(scientificName, nameof(scientificName));
        IdealNdvi = GuardRange(idealNdvi, 0.1m, 1m, nameof(idealNdvi));
        WaterDemandIndex = GuardRange(waterDemandIndex, 0m, 1m, nameof(waterDemandIndex));
    }

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
