using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Services;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Responses;
using SharedLibrary.Routes;

namespace Server.Controllers;

[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly CyclicDependencySolver _cyclicDependencySolver;
    private readonly IHeroMapService _heroMapService;
    public SessionController(ISessionService sessionService, CyclicDependencySolver cyclicDependencySolver, IHeroMapService heroMapService)
    {
        _sessionService = sessionService;
        _cyclicDependencySolver = cyclicDependencySolver;
        _heroMapService = heroMapService;
    }

    [HttpGet, Route(ApiRoutes.Session.GetById)]
    public async Task<IActionResult> GetSessionById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetByIdAsync(id, cancellationToken);
        if (session is null)
            return Ok(new GetSessionResponse { Info = new []{ErrorMessages.Session.NotFound}, Session = null});
        
        _cyclicDependencySolver.Solve(session);
        return Ok(new GetSessionResponse { Info = new []{SuccessMessages.Session.Found}, Session = session});
    }

    #region endpoint for testing functionality

    [HttpGet, Route(ApiRoutes.Session.GetHeroMap)]
    public async Task<IActionResult> GetHeroMap([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var heroMap = await _heroMapService.GetHeroMapAsync(id, cancellationToken);
        if (heroMap is null)
            return NotFound();
        return Ok(heroMap);
    }

    #endregion

}