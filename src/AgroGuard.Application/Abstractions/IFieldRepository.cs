using AgroGuard.Domain.Entities;

namespace AgroGuard.Application.Abstractions;

public interface IFieldRepository
{
    Task<IReadOnlyList<Field>> ListByFarmAsync(Guid farmId, Guid ownerId, CancellationToken cancellationToken);
    Task<Field?> GetOwnedByIdAsync(Guid fieldId, Guid ownerId, CancellationToken cancellationToken);
    Task AddAsync(Field field, CancellationToken cancellationToken);
}
