namespace AgroGuard.Application.Readings;

public interface ISatelliteMonitoringService
{
    Task<ReadingAnalysisResponse> AddReadingAsync(Guid fieldId, CreateSatelliteReadingRequest request, CancellationToken cancellationToken);
}
