using AgroGuard.Domain.Entities;

namespace AgroGuard.Application.Abstractions;

public interface ISatelliteReadingRepository
{
    Task<SatelliteReading?> GetLatestByFieldAsync(Guid fieldId, CancellationToken cancellationToken);
    Task AddAsync(SatelliteReading reading, CancellationToken cancellationToken);
}
