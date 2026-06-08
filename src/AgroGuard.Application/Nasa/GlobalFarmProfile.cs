namespace AgroGuard.Application.Nasa;

internal sealed record GlobalFarmProfile(
    string Code,
    string Name,
    string Country,
    string Region,
    string CropName,
    string ScientificName,
    decimal IdealNdvi,
    decimal WaterDemandIndex,
    decimal Latitude,
    decimal Longitude,
    decimal AreaHectares,
    string SoilType);
