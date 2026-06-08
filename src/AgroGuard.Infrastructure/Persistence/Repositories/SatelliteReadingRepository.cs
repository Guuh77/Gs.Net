using AgroGuard.Application.Abstractions;
using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroGuard.Infrastructure.Persistence.Repositories;

internal sealed class SatelliteReadingRepository : ISatelliteReadingRepository
{
    private readonly AgroGuardDbContext _context;

    public SatelliteReadingRepository(AgroGuardDbContext context)
    {
        _context = context;
    }

    public Task<SatelliteReading?> GetLatestByFieldAsync(Guid fieldId, CancellationToken cancellationToken)
    {
        return _context.SatelliteReadings
            .Where(reading => reading.FieldId == fieldId)
            .OrderByDescending(reading => reading.CapturedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(SatelliteReading reading, CancellationToken cancellationToken)
    {
        await _context.SatelliteReadings.AddAsync(reading, cancellationToken);
    }
}
