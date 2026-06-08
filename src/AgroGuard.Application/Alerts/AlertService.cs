using AgroGuard.Application.Abstractions;
using AgroGuard.Application.Common.Exceptions;
using AgroGuard.Application.Common.Security;
using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Application.Alerts;

public sealed class AlertService : IAlertService
{
    private readonly IAlertRepository _alerts;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AlertService(IAlertRepository alerts, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    {
        _alerts = alerts;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AlertResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var alerts = await _alerts.ListByOwnerAsync(RequireUserId(), cancellationToken);
        return alerts.Select(alert => ToResponse(alert)).ToArray();
    }

    public async Task<IReadOnlyList<AlertResponse>> ListHighRiskAsync(CancellationToken cancellationToken)
    {
        var alerts = await _alerts.ListByOwnerAndLevelsAsync(
            RequireUserId(),
            [RiskLevel.High, RiskLevel.Critical],
            cancellationToken);

        return alerts.Select(alert => ToResponse(alert)).ToArray();
    }

    public async Task<AlertResponse> ResolveAsync(Guid alertId, CancellationToken cancellationToken)
    {
        var alert = await _alerts.GetOwnedByIdAsync(alertId, RequireUserId(), cancellationToken);
        if (alert is null)
        {
            throw new NotFoundException("RiskAlert", alertId);
        }

        alert.Resolve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(alert);
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId ?? throw new ForbiddenException("Authenticated user was not found.");
    }

    public static AlertResponse ToResponse(RiskAlert alert, string? fieldName = null)
    {
        return new AlertResponse(
            alert.Id,
            alert.FieldId,
            fieldName ?? alert.Field?.Name ?? string.Empty,
            alert.Type.ToString(),
            alert.Level.ToString(),
            alert.Score,
            alert.Title,
            alert.Description,
            alert.Recommendation,
            alert.Status.ToString(),
            alert.CreatedAt,
            alert.ResolvedAt);
    }
}
