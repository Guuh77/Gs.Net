using AgroGuard.Domain.Common;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Domain.Entities;

public sealed class RiskAlert : Entity
{
    private RiskAlert()
    {
    }

    public RiskAlert(
        Guid fieldId,
        Guid? satelliteReadingId,
        AlertType type,
        RiskLevel level,
        decimal score,
        string title,
        string description,
        string recommendation)
    {
        FieldId = fieldId;
        SatelliteReadingId = satelliteReadingId;
        Type = type;
        Level = level;
        Score = GuardRange(score, 0m, 100m, nameof(score));
        Title = GuardRequired(title, nameof(title));
        Description = GuardRequired(description, nameof(description));
        Recommendation = GuardRequired(recommendation, nameof(recommendation));
        Status = AlertStatus.Open;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid FieldId { get; private set; }
    public Guid? SatelliteReadingId { get; private set; }
    public AlertType Type { get; private set; }
    public RiskLevel Level { get; private set; }
    public decimal Score { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Recommendation { get; private set; } = string.Empty;
    public AlertStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Field Field { get; private set; } = null!;
    public SatelliteReading? SatelliteReading { get; private set; }

    public void Acknowledge()
    {
        if (Status == AlertStatus.Open)
        {
            Status = AlertStatus.Acknowledged;
        }
    }

    public void Resolve()
    {
        if (Status == AlertStatus.Resolved)
        {
            return;
        }

        Status = AlertStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
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
