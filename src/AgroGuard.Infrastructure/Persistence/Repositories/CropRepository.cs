using AgroGuard.Application.Abstractions;
using AgroGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroGuard.Infrastructure.Persistence.Repositories;

internal sealed class CropRepository : ICropRepository
{
    private readonly AgroGuardDbContext _context;

    public CropRepository(AgroGuardDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Crop>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.Crops
            .AsNoTracking()
            .OrderBy(crop => crop.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Crop?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Crops.FirstOrDefaultAsync(crop => crop.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return _context.Crops.AnyAsync(crop => crop.Name.ToLower() == name.Trim().ToLower(), cancellationToken);
    }

    public async Task AddAsync(Crop crop, CancellationToken cancellationToken)
    {
        await _context.Crops.AddAsync(crop, cancellationToken);
    }
}
