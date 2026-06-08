using AgroGuard.Application.Fields;
using AgroGuard.Application.Readings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fields")]
public sealed class FieldsController : ControllerBase
{
    private readonly IFieldService _fieldService;
    private readonly ISatelliteMonitoringService _satelliteMonitoringService;

    public FieldsController(IFieldService fieldService, ISatelliteMonitoringService satelliteMonitoringService)
    {
        _fieldService = fieldService;
        _satelliteMonitoringService = satelliteMonitoringService;
    }

    [HttpGet("farm/{farmId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<FieldResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FieldResponse>>> ListByFarm(Guid farmId, CancellationToken cancellationToken)
    {
        return Ok(await _fieldService.ListByFarmAsync(farmId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FieldResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _fieldService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(FieldResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FieldResponse>> Create(CreateFieldRequest request, CancellationToken cancellationToken)
    {
        var response = await _fieldService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPost("{id:guid}/readings")]
    [ProducesResponseType(typeof(ReadingAnalysisResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReadingAnalysisResponse>> AddReading(
        Guid id,
        CreateSatelliteReadingRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _satelliteMonitoringService.AddReadingAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, response);
    }
}
