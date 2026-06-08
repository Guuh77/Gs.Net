using AgroGuard.Application.Abstractions;
using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgroGuard.Infrastructure.Persistence.Repositories;

internal sealed class AlertRepository : IAlertRepository
{
    private readonly AgroGuardDbContext _context;

    public AlertRepository(AgroGuardDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RiskAlert>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return await BaseQuery()
            .Where(alert => alert.Field.Farm.OwnerId == ownerId)
            .OrderByDescending(alert => alert.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RiskAlert>> ListByOwnerAndLevelsAsync(
        Guid ownerId,
        RiskLevel[] levels,
        CancellationToken cancellationToken)
    {
        return await BaseQuery()
            .Where(alert =>
                alert.Field.Farm.OwnerId == ownerId &&
                alert.Status != AlertStatus.Resolved &&
                levels.Contains(alert.Level))
            .OrderByDescending(alert => alert.Score)
            .ThenByDescending(alert => alert.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<RiskAlert?> GetOwnedByIdAsync(Guid alertId, Guid ownerId, CancellationToken cancellationToken)
    {
        return BaseQuery()
            .FirstOrDefaultAsync(alert => alert.Id == alertId && alert.Field.Farm.OwnerId == ownerId, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<RiskAlert> alerts, CancellationToken cancellationToken)
    {
        await _context.RiskAlerts.AddRangeAsync(alerts, cancellationToken);
    }

    private IQueryable<RiskAlert> BaseQuery()
    {
        return _context.RiskAlerts
            .Include(alert => alert.Field)
                .ThenInclude(field => field.Farm)
            .Include(alert => alert.SatelliteReading);
    }
}
