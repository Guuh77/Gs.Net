using AgroGuard.Application.Alerts;

namespace AgroGuard.Application.Nasa;

public sealed record GlobalFarmAnalysisResponse(
    string Code,
    string Name,
    string Country,
    string Region,
    string CropName,
    decimal Latitude,
    decimal Longitude,
    decimal AreaHectares,
    string DataSource,
    DateTime StartDate,
    DateTime EndDate,
    decimal EstimatedNdvi,
    decimal SoilMoisturePercent,
    decimal MaximumTemperatureCelsius,
    decimal TotalRainfallMillimeters,
    decimal RelativeHumidityPercent,
    decimal CloudCoveragePercent,
    IReadOnlyCollection<AlertResponse> Alerts);

public sealed record CoordinateAnalysisRequest(
    decimal Latitude,
    decimal Longitude,
    string CropName,
    decimal AreaHectares);

public sealed record NasaClimateDashboardRequest(
    decimal Latitude,
    decimal Longitude,
    string? CropName,
    int Days = 30);

public sealed record NasaClimateDashboardResponse(
    decimal Latitude,
    decimal Longitude,
    string CropName,
    string Source,
    DateTime StartDate,
    DateTime EndDate,
    NasaClimateSummaryResponse Summary,
    NasaVegetationIndexResponse Vegetation,
    IReadOnlyCollection<NasaDailyClimateResponse> Daily);

public sealed record NasaClimateSummaryResponse(
    decimal AverageTemperatureCelsius,
    decimal MaximumTemperatureCelsius,
    decimal MinimumTemperatureCelsius,
    decimal TotalRainfallMillimeters,
    decimal AverageRelativeHumidityPercent,
    decimal AverageSolarRadiationMjPerSquareMeter,
    decimal AverageSoilMoisturePercent,
    decimal AverageCloudCoveragePercent,
    int DaysWithRain,
    int DaysWithoutRain);

public sealed record NasaDailyClimateResponse(
    DateTime Date,
    decimal TemperatureCelsius,
    decimal MaximumTemperatureCelsius,
    decimal MinimumTemperatureCelsius,
    decimal RainfallMillimeters,
    decimal RelativeHumidityPercent,
    decimal SolarRadiationMjPerSquareMeter,
    decimal SoilMoisturePercent,
    decimal CloudCoveragePercent);

public sealed record NasaVegetationIndexResponse(
    decimal Ndvi,
    string Classification,
    string Description,
    string Color,
    decimal ScorePercent);

public sealed record NasaNaturalEventsResponse(
    int Total,
    IReadOnlyCollection<NasaNaturalEventResponse> Events);

public sealed record NasaNaturalEventResponse(
    string Id,
    string Title,
    string CategoryId,
    string CategoryName,
    string CategoryLabel,
    DateTime? Date,
    decimal? Latitude,
    decimal? Longitude,
    decimal? Magnitude,
    string? MagnitudeUnit,
    string? SourceUrl);

public sealed record NasaAssistantChatRequest(
    string Message,
    NasaClimateDashboardResponse? Climate,
    IReadOnlyCollection<NasaNaturalEventResponse>? Events);

public sealed record NasaAssistantChatResponse(
    string Response,
    DateTime Timestamp);
