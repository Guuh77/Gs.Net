namespace AgroGuard.Application.Alerts;

public sealed record AlertResponse(
    Guid Id,
    Guid FieldId,
    string FieldName,
    string Type,
    string Level,
    decimal Score,
    string Title,
    string Description,
    string Recommendation,
    string Status,
    DateTime CreatedAt,
    DateTime? ResolvedAt);
