using AgroGuard.Application.Abstractions;
using AgroGuard.Application.Common.Exceptions;
using AgroGuard.Application.Common.Security;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IFarmRepository _farms;
    private readonly IAlertRepository _alerts;
    private readonly ICurrentUser _currentUser;

    public DashboardService(IFarmRepository farms, IAlertRepository alerts, ICurrentUser currentUser)
    {
        _farms = farms;
        _alerts = alerts;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var ownerId = _currentUser.UserId ?? throw new ForbiddenException("Authenticated user was not found.");
        var farms = await _farms.ListByOwnerAsync(ownerId, cancellationToken);
        var alerts = await _alerts.ListByOwnerAsync(ownerId, cancellationToken);
        var fields = farms.SelectMany(farm => farm.Fields).ToArray();
        var latestReadings = fields
            .Select(field => field.SatelliteReadings.OrderByDescending(reading => reading.CapturedAt).FirstOrDefault())
            .Where(reading => reading is not null)
            .ToArray();

        return new DashboardSummaryResponse(
            farms.Count,
            fields.Length,
            alerts.Count(alert => alert.Status != AlertStatus.Resolved),
            alerts.Count(alert => alert.Status != AlertStatus.Resolved && alert.Level is RiskLevel.High or RiskLevel.Critical),
            latestReadings.Length == 0 ? 0m : decimal.Round(latestReadings.Average(reading => reading!.Ndvi), 3),
            latestReadings.Length == 0 ? 0m : decimal.Round(latestReadings.Average(reading => reading!.SoilMoisturePercent), 2),
            latestReadings.Length == 0 ? null : latestReadings.Max(reading => reading?.CapturedAt));
    }
}
