namespace AgroGuard.Application.Farms;

public interface IFarmService
{
    Task<IReadOnlyList<FarmResponse>> ListAsync(CancellationToken cancellationToken);
    Task<FarmResponse> CreateAsync(CreateFarmRequest request, CancellationToken cancellationToken);
    Task<FarmResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
