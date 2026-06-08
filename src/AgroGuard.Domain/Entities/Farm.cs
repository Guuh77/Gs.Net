using AgroGuard.Domain.Common;

namespace AgroGuard.Domain.Entities;

public sealed class Farm : Entity
{
    private readonly List<Field> _fields = [];

    private Farm()
    {
    }

    public Farm(Guid ownerId, string name, string city, string state, decimal latitude, decimal longitude, decimal totalAreaHectares)
    {
        OwnerId = ownerId;
        Update(name, city, state, latitude, longitude, totalAreaHectares);
        CreatedAt = DateTime.UtcNow;
    }

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public decimal TotalAreaHectares { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public User Owner { get; private set; } = null!;
    public IReadOnlyCollection<Field> Fields => _fields;

    public void Update(string name, string city, string state, decimal latitude, decimal longitude, decimal totalAreaHectares)
    {
        Name = GuardRequired(name, nameof(name));
        City = GuardRequired(city, nameof(city));
        State = GuardRequired(state, nameof(state)).ToUpperInvariant();
        Latitude = GuardRange(latitude, -90m, 90m, nameof(latitude));
        Longitude = GuardRange(longitude, -180m, 180m, nameof(longitude));

        if (totalAreaHectares <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAreaHectares), totalAreaHectares, "Farm area must be positive.");
        }

        TotalAreaHectares = totalAreaHectares;
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
