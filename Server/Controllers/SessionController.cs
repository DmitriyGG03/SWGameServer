using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Repositories;
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
    private readonly IGameObjectsRepository _gameObjectsRepository;
    public SessionController(ISessionService sessionService, CyclicDependencySolver cyclicDependencySolver, IHeroMapService heroMapService, IGameObjectsRepository gameObjectsRepository)
    {
        _sessionService = sessionService;
        _cyclicDependencySolver = cyclicDependencySolver;
        _heroMapService = heroMapService;
        _gameObjectsRepository = gameObjectsRepository;
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

    #region endpoints for testing functionality

    [HttpGet, Route(ApiRoutes.Session.GetHeroMap)]
    public async Task<IActionResult> GetHeroMap([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var heroMap = await _heroMapService.GetHeroMapAsync(id, cancellationToken);
        if (heroMap is null)
            return NotFound();
        return Ok(heroMap);
    }
    
    [HttpGet, Route(ApiRoutes.Session.GetBattles)]
    public async Task<IActionResult> GetBattles(CancellationToken cancellationToken)
    {
        var battles = await _gameObjectsRepository.GetBattlesAsync(cancellationToken);
        return Ok(battles);
    }
    
    #endregion

}