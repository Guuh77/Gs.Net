using AgroGuard.Application.Nasa;
using AgroGuard.Application.Readings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/nasa")]
public sealed class NasaController : ControllerBase
{
    private readonly INasaMonitoringService _nasaMonitoringService;

    public NasaController(INasaMonitoringService nasaMonitoringService)
    {
        _nasaMonitoringService = nasaMonitoringService;
    }

    [HttpGet("global-farms")]
    [ProducesResponseType(typeof(IReadOnlyList<GlobalFarmAnalysisResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GlobalFarmAnalysisResponse>>> GlobalFarms(CancellationToken cancellationToken)
    {
        return Ok(await _nasaMonitoringService.AnalyzeGlobalFarmsAsync(cancellationToken));
    }

    [HttpPost("coordinates/analyze")]
    [ProducesResponseType(typeof(GlobalFarmAnalysisResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GlobalFarmAnalysisResponse>> AnalyzeCoordinates(
        CoordinateAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _nasaMonitoringService.AnalyzeCoordinatesAsync(request, cancellationToken));
    }

    [HttpPost("fields/{fieldId:guid}/analyze")]
    [ProducesResponseType(typeof(ReadingAnalysisResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReadingAnalysisResponse>> AnalyzeOwnedField(
        Guid fieldId,
        CancellationToken cancellationToken)
    {
        return Created($"/api/fields/{fieldId}", await _nasaMonitoringService.AnalyzeOwnedFieldWithNasaAsync(fieldId, cancellationToken));
    }

    [HttpGet("climate")]
    [ProducesResponseType(typeof(NasaClimateDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NasaClimateDashboardResponse>> Climate(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] string? cropName,
        [FromQuery] int days,
        CancellationToken cancellationToken)
    {
        var request = new NasaClimateDashboardRequest(
            latitude,
            longitude,
            cropName,
            days <= 0 ? 30 : days);

        return Ok(await _nasaMonitoringService.GetClimateDashboardAsync(request, cancellationToken));
    }

    [HttpGet("events")]
    [ProducesResponseType(typeof(NasaNaturalEventsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NasaNaturalEventsResponse>> Events(
        [FromQuery] int days,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        return Ok(await _nasaMonitoringService.GetNaturalEventsAsync(days <= 0 ? 60 : days, category, cancellationToken));
    }

    [HttpPost("assistant/chat")]
    [ProducesResponseType(typeof(NasaAssistantChatResponse), StatusCodes.Status200OK)]
    public ActionResult<NasaAssistantChatResponse> AssistantChat(NasaAssistantChatRequest request)
    {
        return Ok(_nasaMonitoringService.GenerateAssistantReply(request));
    }
}
