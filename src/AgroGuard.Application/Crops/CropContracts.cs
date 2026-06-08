namespace AgroGuard.Application.Crops;

public sealed record CreateCropRequest(
    string Name,
    string ScientificName,
    decimal IdealNdvi,
    decimal WaterDemandIndex);

public sealed record CropResponse(
    Guid Id,
    string Name,
    string ScientificName,
    decimal IdealNdvi,
    decimal WaterDemandIndex);
