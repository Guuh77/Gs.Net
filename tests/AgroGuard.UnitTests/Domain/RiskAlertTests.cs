using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;

namespace AgroGuard.UnitTests.Domain;

public sealed class RiskAlertTests
{
    [Fact]
    public void Resolve_WhenAlertIsOpen_MarksAlertAsResolved()
    {
        // Arrange
        var alert = new RiskAlert(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AlertType.Wildfire,
            RiskLevel.High,
            82m,
            "Wildfire risk detected",
            "High temperature and low humidity.",
            "Inspect the field and activate prevention routines.");

        // Act
        alert.Resolve();

        // Assert
        Assert.Equal(AlertStatus.Resolved, alert.Status);
        Assert.NotNull(alert.ResolvedAt);
    }
}
