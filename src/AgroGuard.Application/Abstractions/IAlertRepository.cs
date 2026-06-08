using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Application.Abstractions;

public interface IAlertRepository
{
    Task<IReadOnlyList<RiskAlert>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RiskAlert>> ListByOwnerAndLevelsAsync(Guid ownerId, RiskLevel[] levels, CancellationToken cancellationToken);
    Task<RiskAlert?> GetOwnedByIdAsync(Guid alertId, Guid ownerId, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<RiskAlert> alerts, CancellationToken cancellationToken);
}
