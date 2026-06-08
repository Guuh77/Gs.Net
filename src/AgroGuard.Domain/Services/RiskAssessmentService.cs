using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Domain.Services;

public sealed class RiskAssessmentService
{
    private const decimal MinimumAlertScore = 45m;

    public IReadOnlyCollection<RiskAssessment> Assess(
        Field field,
        SatelliteReading currentReading,
        SatelliteReading? previousReading = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(currentReading);

        var assessments = new List<RiskAssessment>
        {
            BuildDroughtAssessment(field, currentReading),
            BuildWildfireAssessment(currentReading),
            BuildFloodAssessment(currentReading)
        };

        var productivityAssessment = BuildProductivityAssessment(field, currentReading, previousReading);
        if (productivityAssessment is not null)
        {
            assessments.Add(productivityAssessment);
        }

        return assessments
            .Where(assessment => assessment.Score >= MinimumAlertScore)
            .OrderByDescending(assessment => assessment.Score)
            .ToArray();
    }

    private static RiskAssessment BuildDroughtAssessment(Field field, SatelliteReading reading)
    {
        var cropDemandMultiplier = 1m + (field.Crop.WaterDemandIndex * 0.2m);
        var ndviGap = Math.Max(0m, field.Crop.IdealNdvi - reading.Ndvi);
        var score =
            ((100m - reading.SoilMoisturePercent) * 0.42m * cropDemandMultiplier) +
            (ndviGap * 100m * 0.28m) +
            (Math.Max(0m, reading.SurfaceTemperatureCelsius - 30m) * 2.2m) +
            (Math.Max(0m, 12m - reading.RainfallMillimeters) * 1.5m);

        score = ClampScore(score);

        return new RiskAssessment(
            AlertType.Drought,
            ToRiskLevel(score),
            score,
            "Drought stress detected",
            "Low soil moisture combined with vegetation stress indicates water deficit in the monitored field.",
            "Prioritize irrigation review, inspect crop stress in the field and compare with the next satellite pass.");
    }

    private static RiskAssessment BuildWildfireAssessment(SatelliteReading reading)
    {
        var score =
            (Math.Max(0m, reading.SurfaceTemperatureCelsius - 32m) * 4m) +
            ((100m - reading.SoilMoisturePercent) * 0.34m) +
            (Math.Max(0m, 10m - reading.RainfallMillimeters) * 2.4m);

        score = ClampScore(score);

        return new RiskAssessment(
            AlertType.Wildfire,
            ToRiskLevel(score),
            score,
            "Wildfire risk detected",
            "High surface temperature, low moisture and low recent rainfall increase the probability of fire spread.",
            "Check firebreaks, avoid machinery during the hottest period and notify the local prevention team if risk persists.");
    }

    private static RiskAssessment BuildFloodAssessment(SatelliteReading reading)
    {
        var score =
            (Math.Max(0m, reading.RainfallMillimeters - 35m) * 1.15m) +
            (Math.Max(0m, reading.SoilMoisturePercent - 70m) * 1.55m) +
            (reading.CloudCoveragePercent * 0.08m);

        score = ClampScore(score);

        return new RiskAssessment(
            AlertType.Flood,
            ToRiskLevel(score),
            score,
            "Flood risk detected",
            "High rainfall and saturated soil conditions suggest possible flooding or drainage failure.",
            "Inspect drainage channels, protect access roads and verify low-lying crop areas before field operations.");
    }

    private static RiskAssessment? BuildProductivityAssessment(
        Field field,
        SatelliteReading currentReading,
        SatelliteReading? previousReading)
    {
        var expectedGapScore = Math.Max(0m, field.Crop.IdealNdvi - currentReading.Ndvi) * 100m;
        var trendDropScore = previousReading is null
            ? 0m
            : Math.Max(0m, previousReading.Ndvi - currentReading.Ndvi) * 180m;

        var score = ClampScore((expectedGapScore * 0.62m) + (trendDropScore * 0.38m));

        if (score < MinimumAlertScore)
        {
            return null;
        }

        return new RiskAssessment(
            AlertType.ProductivityDrop,
            ToRiskLevel(score),
            score,
            "Productivity loss trend detected",
            "Vegetation index is below the crop target or dropped sharply compared with the previous reading.",
            "Compare the area with field scouting, pest pressure and fertilizer history before estimating production loss.");
    }

    private static RiskLevel ToRiskLevel(decimal score)
    {
        return score switch
        {
            >= 90m => RiskLevel.Critical,
            >= 70m => RiskLevel.High,
            >= 45m => RiskLevel.Moderate,
            _ => RiskLevel.Low
        };
    }

    private static decimal ClampScore(decimal score)
    {
        return Math.Clamp(decimal.Round(score, 2, MidpointRounding.AwayFromZero), 0m, 100m);
    }
}
