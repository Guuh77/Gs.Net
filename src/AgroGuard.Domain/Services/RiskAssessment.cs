using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Domain.Services;

public sealed record RiskAssessment(
    AlertType Type,
    RiskLevel Level,
    decimal Score,
    string Title,
    string Description,
    string Recommendation)
{
    public RiskAlert ToAlert(Guid fieldId, Guid? satelliteReadingId)
    {
        return new RiskAlert(fieldId, satelliteReadingId, Type, Level, Score, Title, Description, Recommendation);
    }
}
