using AgroGuard.Application.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerts")]
public sealed class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _alertService.ListAsync(cancellationToken));
    }

    [HttpGet("high-risk")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> ListHighRisk(CancellationToken cancellationToken)
    {
        return Ok(await _alertService.ListHighRiskAsync(cancellationToken));
    }

    [HttpPatch("{id:guid}/resolve")]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlertResponse>> Resolve(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _alertService.ResolveAsync(id, cancellationToken));
    }
}
