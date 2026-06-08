namespace AgroGuard.Application.Farms;

public sealed record CreateFarmRequest(
    string Name,
    string City,
    string State,
    decimal Latitude,
    decimal Longitude,
    decimal TotalAreaHectares);

public sealed record FarmResponse(
    Guid Id,
    string Name,
    string City,
    string State,
    decimal Latitude,
    decimal Longitude,
    decimal TotalAreaHectares,
    int FieldCount,
    DateTime CreatedAt);
