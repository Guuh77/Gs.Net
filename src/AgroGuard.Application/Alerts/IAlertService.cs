namespace AgroGuard.Application.Alerts;

public interface IAlertService
{
    Task<IReadOnlyList<AlertResponse>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AlertResponse>> ListHighRiskAsync(CancellationToken cancellationToken);
    Task<AlertResponse> ResolveAsync(Guid alertId, CancellationToken cancellationToken);
}
