using AgroGuard.Application.Alerts;

namespace AgroGuard.Application.Readings;

public sealed record CreateSatelliteReadingRequest(
    DateTime? CapturedAt,
    string Source,
    decimal Ndvi,
    decimal SoilMoisturePercent,
    decimal SurfaceTemperatureCelsius,
    decimal RainfallMillimeters,
    decimal CloudCoveragePercent);

public sealed record SatelliteReadingResponse(
    Guid Id,
    Guid FieldId,
    DateTime CapturedAt,
    string Source,
    decimal Ndvi,
    decimal SoilMoisturePercent,
    decimal SurfaceTemperatureCelsius,
    decimal RainfallMillimeters,
    decimal CloudCoveragePercent);

public sealed record ReadingAnalysisResponse(
    SatelliteReadingResponse Reading,
    IReadOnlyCollection<AlertResponse> GeneratedAlerts);
