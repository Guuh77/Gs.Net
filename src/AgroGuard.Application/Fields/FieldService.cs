using AgroGuard.Application.Abstractions;
using AgroGuard.Application.Common.Exceptions;
using AgroGuard.Application.Common.Security;
using AgroGuard.Domain.Entities;
using AgroGuard.Domain.Enums;

namespace AgroGuard.Application.Fields;

public sealed class FieldService : IFieldService
{
    private readonly IFieldRepository _fields;
    private readonly IFarmRepository _farms;
    private readonly ICropRepository _crops;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public FieldService(
        IFieldRepository fields,
        IFarmRepository farms,
        ICropRepository crops,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _fields = fields;
        _farms = farms;
        _crops = crops;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<FieldResponse>> ListByFarmAsync(Guid farmId, CancellationToken cancellationToken)
    {
        await EnsureOwnedFarmAsync(farmId, cancellationToken);
        var fields = await _fields.ListByFarmAsync(farmId, RequireUserId(), cancellationToken);
        return fields.Select(ToResponse).ToArray();
    }

    public async Task<FieldResponse> CreateAsync(CreateFieldRequest request, CancellationToken cancellationToken)
    {
        await EnsureOwnedFarmAsync(request.FarmId, cancellationToken);

        var crop = await _crops.GetByIdAsync(request.CropId, cancellationToken);
        if (crop is null)
        {
            throw new NotFoundException("Crop", request.CropId);
        }

        var field = new Field(
            request.FarmId,
            crop.Id,
            request.Name,
            request.AreaHectares,
            request.Latitude,
            request.Longitude,
            request.SoilType,
            request.PlantedAt,
            request.ExpectedHarvestAt);

        await _fields.AddAsync(field, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(field.Id, cancellationToken);
    }

    public async Task<FieldResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var field = await _fields.GetOwnedByIdAsync(id, RequireUserId(), cancellationToken);
        return field is null ? throw new NotFoundException("Field", id) : ToResponse(field);
    }

    private async Task EnsureOwnedFarmAsync(Guid farmId, CancellationToken cancellationToken)
    {
        var farm = await _farms.GetOwnedByIdAsync(farmId, RequireUserId(), cancellationToken);
        if (farm is null)
        {
            throw new NotFoundException("Farm", farmId);
        }
    }

    private Guid RequireUserId()
    {
        return _currentUser.UserId ?? throw new ForbiddenException("Authenticated user was not found.");
    }

    private static FieldResponse ToResponse(Field field)
    {
        var latestReading = field.SatelliteReadings.OrderByDescending(reading => reading.CapturedAt).FirstOrDefault();
        var openAlertCount = field.RiskAlerts.Count(alert => alert.Status != AlertStatus.Resolved);

        return new FieldResponse(
            field.Id,
            field.FarmId,
            field.Farm.Name,
            field.CropId,
            field.Crop.Name,
            field.Name,
            field.AreaHectares,
            field.Latitude,
            field.Longitude,
            field.SoilType,
            field.PlantedAt,
            field.ExpectedHarvestAt,
            latestReading?.Ndvi,
            openAlertCount);
    }
}
