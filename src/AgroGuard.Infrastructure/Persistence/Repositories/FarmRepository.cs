using AgroGuard.Application.Abstractions;
using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroGuard.Infrastructure.Persistence.Repositories;

internal sealed class FarmRepository : IFarmRepository
{
    private readonly AgroGuardDbContext _context;

    public FarmRepository(AgroGuardDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Farm>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        return await _context.Farms
            .Include(farm => farm.Fields)
                .ThenInclude(field => field.Crop)
            .Include(farm => farm.Fields)
                .ThenInclude(field => field.SatelliteReadings)
            .Include(farm => farm.Fields)
                .ThenInclude(field => field.RiskAlerts)
            .Where(farm => farm.OwnerId == ownerId)
            .OrderBy(farm => farm.Name)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public Task<Farm?> GetOwnedByIdAsync(Guid farmId, Guid ownerId, CancellationToken cancellationToken)
    {
        return _context.Farms
            .Include(farm => farm.Fields)
            .FirstOrDefaultAsync(farm => farm.Id == farmId && farm.OwnerId == ownerId, cancellationToken);
    }

    public async Task AddAsync(Farm farm, CancellationToken cancellationToken)
    {
        await _context.Farms.AddAsync(farm, cancellationToken);
    }
}
