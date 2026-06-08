using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;
using AgroGuard.Domain.Services;

namespace AgroGuard.UnitTests.Domain;

public sealed class RiskAssessmentServiceTests
{
    private readonly RiskAssessmentService _sut = new();

    [Fact]
    public void Assess_WhenReadingShowsWaterStress_ReturnsDroughtAlert()
    {
        // Arrange
        var field = CreateField();
        var reading = CreateReading(ndvi: 0.42m, soilMoisture: 18m, temperature: 39m, rainfall: 0m);

        // Act
        var result = _sut.Assess(field, reading);

        // Assert
        var droughtAlert = Assert.Single(result.Where(alert => alert.Type == AlertType.Drought));
        Assert.True(droughtAlert.Score >= 70m);
        Assert.True(droughtAlert.Level is RiskLevel.High or RiskLevel.Critical);
    }

    [Fact]
    public void Assess_WhenReadingShowsSaturatedSoil_ReturnsFloodAlert()
    {
        // Arrange
        var field = CreateField();
        var reading = CreateReading(ndvi: 0.74m, soilMoisture: 92m, temperature: 23m, rainfall: 115m, cloudCoverage: 96m);

        // Act
        var result = _sut.Assess(field, reading);

        // Assert
        var floodAlert = Assert.Single(result.Where(alert => alert.Type == AlertType.Flood));
        Assert.True(floodAlert.Score >= 70m);
        Assert.Equal("Flood risk detected", floodAlert.Title);
    }

    [Fact]
    public void Assess_WhenNdviDropsSharply_ReturnsProductivityDropAlert()
    {
        // Arrange
        var field = CreateField();
        var previous = CreateReading(ndvi: 0.82m, soilMoisture: 58m, temperature: 28m, rainfall: 18m);
        var current = CreateReading(ndvi: 0.43m, soilMoisture: 55m, temperature: 30m, rainfall: 15m);

        // Act
        var result = _sut.Assess(field, current, previous);

        // Assert
        var productivityAlert = Assert.Single(result.Where(alert => alert.Type == AlertType.ProductivityDrop));
        Assert.True(productivityAlert.Score >= 45m);
        Assert.Contains("Vegetation index", productivityAlert.Description);
    }

    [Fact]
    public void Assess_WhenReadingIsHealthy_DoesNotReturnAlerts()
    {
        // Arrange
        var field = CreateField();
        var reading = CreateReading(ndvi: 0.84m, soilMoisture: 61m, temperature: 27m, rainfall: 26m);

        // Act
        var result = _sut.Assess(field, reading);

        // Assert
        Assert.Empty(result);
    }

    private static Field CreateField()
    {
        var crop = new Crop("Soybean", "Glycine max", 0.82m, 0.78m);

        return new Field(
            Guid.NewGuid(),
            crop,
            "Field A",
            120m,
            -21.1775m,
            -47.8103m,
            "Clay",
            new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc));
    }

    private static SatelliteReading CreateReading(
        decimal ndvi,
        decimal soilMoisture,
        decimal temperature,
        decimal rainfall,
        decimal cloudCoverage = 20m)
    {
        return new SatelliteReading(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Sentinel-2 Academic Sample",
            ndvi,
            soilMoisture,
            temperature,
            rainfall,
            cloudCoverage);
    }
}
