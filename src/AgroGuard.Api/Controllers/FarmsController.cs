using AgroGuard.Application.Farms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/farms")]
public sealed class FarmsController : ControllerBase
{
    private readonly IFarmService _farmService;

    public FarmsController(IFarmService farmService)
    {
        _farmService = farmService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FarmResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FarmResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _farmService.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FarmResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _farmService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<FarmResponse>> Create(CreateFarmRequest request, CancellationToken cancellationToken)
    {
        var response = await _farmService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }
}
