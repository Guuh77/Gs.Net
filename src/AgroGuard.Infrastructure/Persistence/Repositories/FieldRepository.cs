using AgroGuard.Application.Abstractions;
using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroGuard.Infrastructure.Persistence.Repositories;

internal sealed class FieldRepository : IFieldRepository
{
    private readonly AgroGuardDbContext _context;

    public FieldRepository(AgroGuardDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Field>> ListByFarmAsync(Guid farmId, Guid ownerId, CancellationToken cancellationToken)
    {
        return await BaseQuery()
            .Where(field => field.FarmId == farmId && field.Farm.OwnerId == ownerId)
            .OrderBy(field => field.Name)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public Task<Field?> GetOwnedByIdAsync(Guid fieldId, Guid ownerId, CancellationToken cancellationToken)
    {
        return BaseQuery()
            .AsSplitQuery()
            .FirstOrDefaultAsync(field => field.Id == fieldId && field.Farm.OwnerId == ownerId, cancellationToken);
    }

    public async Task AddAsync(Field field, CancellationToken cancellationToken)
    {
        await _context.Fields.AddAsync(field, cancellationToken);
    }

    private IQueryable<Field> BaseQuery()
    {
        return _context.Fields
            .Include(field => field.Farm)
            .Include(field => field.Crop)
            .Include(field => field.SatelliteReadings)
            .Include(field => field.RiskAlerts);
    }
}
