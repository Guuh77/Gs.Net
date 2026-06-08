using AgroGuard.Application.Abstractions;
using AgroGuard.Application.Common.Exceptions;
using AgroGuard.Application.Common.Security;
using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Application.Crops;

public sealed class CropService : ICropService
{
    private readonly ICropRepository _crops;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public CropService(ICropRepository crops, ICurrentUser currentUser, IUnitOfWork unitOfWork)
    {
        _crops = crops;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CropResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var crops = await _crops.ListAsync(cancellationToken);
        return crops.Select(ToResponse).ToArray();
    }

    public async Task<CropResponse> CreateAsync(CreateCropRequest request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role is not (UserRole.Administrator or UserRole.Analyst))
        {
            throw new ForbiddenException("Only administrators or analysts can create crop profiles.");
        }

        if (await _crops.ExistsByNameAsync(request.Name, cancellationToken))
        {
            throw new ConflictException("Crop already registered.");
        }

        var crop = new Crop(request.Name, request.ScientificName, request.IdealNdvi, request.WaterDemandIndex);
        await _crops.AddAsync(crop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(crop);
    }

    private static CropResponse ToResponse(Crop crop)
    {
        return new CropResponse(crop.Id, crop.Name, crop.ScientificName, crop.IdealNdvi, crop.WaterDemandIndex);
    }
}
