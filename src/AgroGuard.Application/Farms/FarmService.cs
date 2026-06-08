using AgroGuard.Application.Abstractions;
using AgroGuard.Application.Common.Exceptions;
using AgroGuard.Application.Common.Security;
using AgroGuard.Domain.Entities;

namespace AgroGuard.Application.Farms;

public sealed class FarmService : IFarmService
{
    private readonly IFarmRepository _farms;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public FarmService(IFarmRepository farms, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    {
        _farms = farms;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<FarmResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var ownerId = RequireUserId();
        var farms = await _farms.ListByOwnerAsync(ownerId, cancellationToken);
        return farms.Select(ToResponse).ToArray();
    }

    public async Task<FarmResponse> CreateAsync(CreateFarmRequest request, CancellationToken cancellationToken)
    {
        var farm = new Farm(
            RequireUserId(),
            request.Name,
            request.City,
            request.State,
            request.Latitude,
            request.Longitude,
            request.TotalAreaHectares);

        await _farms.AddAsync(farm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(farm);
    }

    public async Task<FarmResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var farm = await _farms.GetOwnedByIdAsync(id, RequireUserId(), cancellationToken);
        return farm is null ? throw new NotFoundException("Farm", id) : ToResponse(farm);
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId ?? throw new ForbiddenException("Authenticated user was not found.");
    }

    private static FarmResponse ToResponse(Farm farm)
    {
        return new FarmResponse(
            farm.Id,
            farm.Name,
            farm.City,
            farm.State,
            farm.Latitude,
            farm.Longitude,
            farm.TotalAreaHectares,
            farm.Fields.Count,
            farm.CreatedAt);
    }
}
