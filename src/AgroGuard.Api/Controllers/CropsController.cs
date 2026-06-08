using AgroGuard.Application.Crops;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/crops")]
public sealed class CropsController : ControllerBase
{
    private readonly ICropService _cropService;

    public CropsController(ICropService cropService)
    {
        _cropService = cropService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CropResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CropResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _cropService.ListAsync(cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Analyst")]
    [ProducesResponseType(typeof(CropResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CropResponse>> Create(CreateCropRequest request, CancellationToken cancellationToken)
    {
        var response = await _cropService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(List), new { id = response.Id }, response);
    }
}
