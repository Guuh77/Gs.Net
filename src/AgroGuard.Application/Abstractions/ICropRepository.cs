using AgroGuard.Domain.Entities;

namespace AgroGuard.Application.Abstractions;

public interface ICropRepository
{
    Task<IReadOnlyList<Crop>> ListAsync(CancellationToken cancellationToken);
    Task<Crop?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken);
    Task AddAsync(Crop crop, CancellationToken cancellationToken);
}
