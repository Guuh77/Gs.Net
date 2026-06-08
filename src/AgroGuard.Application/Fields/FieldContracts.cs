namespace AgroGuard.Application.Fields;

public sealed record CreateFieldRequest(
    Guid FarmId,
    Guid CropId,
    string Name,
    decimal AreaHectares,
    decimal Latitude,
    decimal Longitude,
    string SoilType,
    DateTime PlantedAt,
    DateTime ExpectedHarvestAt);

public sealed record FieldResponse(
    Guid Id,
    Guid FarmId,
    string FarmName,
    Guid CropId,
    string CropName,
    string Name,
    decimal AreaHectares,
    decimal Latitude,
    decimal Longitude,
    string SoilType,
    DateTime PlantedAt,
    DateTime ExpectedHarvestAt,
    decimal? LatestNdvi,
    int OpenAlertCount);
