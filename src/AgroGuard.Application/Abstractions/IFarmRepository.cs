using AgroGuard.Domain.Entities;

namespace AgroGuard.Application.Abstractions;

public interface IFarmRepository
{
    Task<IReadOnlyList<Farm>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken);
    Task<Farm?> GetOwnedByIdAsync(Guid farmId, Guid ownerId, CancellationToken cancellationToken);
    Task AddAsync(Farm farm, CancellationToken cancellationToken);
}
