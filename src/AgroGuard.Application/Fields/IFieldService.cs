namespace AgroGuard.Application.Fields;

public interface IFieldService
{
    Task<IReadOnlyList<FieldResponse>> ListByFarmAsync(Guid farmId, CancellationToken cancellationToken);
    Task<FieldResponse> CreateAsync(CreateFieldRequest request, CancellationToken cancellationToken);
    Task<FieldResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
