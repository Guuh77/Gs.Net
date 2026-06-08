using AgroGuard.Application.Readings;

namespace AgroGuard.Application.Nasa;

public interface INasaMonitoringService
{
    Task<IReadOnlyList<GlobalFarmAnalysisResponse>> AnalyzeGlobalFarmsAsync(CancellationToken cancellationToken);
    Task<GlobalFarmAnalysisResponse> AnalyzeCoordinatesAsync(CoordinateAnalysisRequest request, CancellationToken cancellationToken);
    Task<ReadingAnalysisResponse> AnalyzeOwnedFieldWithNasaAsync(Guid fieldId, CancellationToken cancellationToken);
    Task<NasaClimateDashboardResponse> GetClimateDashboardAsync(NasaClimateDashboardRequest request, CancellationToken cancellationToken);
    Task<NasaNaturalEventsResponse> GetNaturalEventsAsync(int days, string? category, CancellationToken cancellationToken);
    NasaAssistantChatResponse GenerateAssistantReply(NasaAssistantChatRequest request);
}
