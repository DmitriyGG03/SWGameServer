using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Responses;
using SharedLibrary.Routes;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MapController : ControllerBase
{
    private readonly IMapService _mapService;
    public MapController(IMapService mapService)
    {
        _mapService = mapService;
    }

    [HttpGet, Route(ApiRoutes.Map.GetMap)]
    public async Task<IActionResult> GetHeroMap([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        /* in future options can be add as body parameter */
        var defaultOptions = new MapGenerationOptions(800, 600, 50, 25, 60);
        var serviceResponse = await _mapService.GetMapAsync(id, defaultOptions, cancellationToken);
        if (serviceResponse.Success == false)
            return BadRequest(new GetMapResponse { Map = null, Info = new[] { serviceResponse.ErrorMessage } });

        return Ok(new GetMapResponse { Map = serviceResponse.Value, Info = null });
    }
}