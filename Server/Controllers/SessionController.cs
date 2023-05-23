using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Responses;
using SharedLibrary.Routes;

namespace Server.Controllers;

[Route("[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet, Route(ApiRoutes.Session.GetById)]
    public async Task<IActionResult> GetSessionById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetByIdAsync(id, cancellationToken);
        if (session is null)
            return Ok(new GetSessionResponse { Info = new []{ErrorMessages.Session.NotFound}, Session = null});
        
        SolveCyclicDependency(session);
        return Ok(new GetSessionResponse { Info = new []{SuccessMessages.Session.Found}, Session = session});
    }

    #region endpoint for testing functionality

    [HttpGet, Route(ApiRoutes.Session.GetHeroMap)]
    public async Task<IActionResult> GetHeroMap([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var heroMap = await _sessionService.GetHeroMapAsync(id, cancellationToken);
        if (heroMap is null)
            return NotFound();
        return Ok(heroMap);
    }

    #endregion
    
    private void SolveCyclicDependency(Session sessionToSolve)
    {
        if (sessionToSolve.Heroes != null)
        {
            foreach (var item in sessionToSolve.Heroes)
            {
                // solve cyclic dependency
                item.User = null;

                item.Session = null;
            }
        }

        if (sessionToSolve.SessionMap != null) 
            sessionToSolve.SessionMap.Session = null;
    }
}