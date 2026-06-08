namespace AgroGuard.Application.Dashboard;

public sealed record DashboardSummaryResponse(
    int Farms,
    int Fields,
    int OpenAlerts,
    int HighRiskAlerts,
    decimal AverageNdvi,
    decimal AverageSoilMoisture,
    DateTime? LatestReadingAt);
