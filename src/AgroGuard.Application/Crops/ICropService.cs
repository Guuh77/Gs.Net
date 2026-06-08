namespace AgroGuard.Application.Crops;

public interface ICropService
{
    Task<IReadOnlyList<CropResponse>> ListAsync(CancellationToken cancellationToken);
    Task<CropResponse> CreateAsync(CreateCropRequest request, CancellationToken cancellationToken);
}
