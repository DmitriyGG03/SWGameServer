using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Services;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using SharedLibrary.Routes;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class HeroController : ControllerBase
{
    private readonly IHeroService _heroService;
    private readonly ILogger<HeroController> _logger;
    private readonly ISessionService _sessionService;
    private readonly IHeroMapService _heroMapService;
    public HeroController(IHeroService heroService, ILogger<HeroController> logger, ISessionService sessionService, IHeroMapService heroMapService)
    {
        _heroService = heroService;
        _logger = logger;
        _sessionService = sessionService;
        _heroMapService = heroMapService;
    }

    [HttpPut, Route(ApiRoutes.Hero.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CreateHeroRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        // can update only name
        var destination = new Hero { HeroId = id, Name = request.Name, };
        var result = await _heroService.Update(userId, destination, cancellationToken);
        
        if (result.Success == false)
        {
            _logger.LogWarning("Can not update hero: " + result.ErrorMessage);
            if (result.ErrorMessage == ErrorMessages.User.HasNoAccess)
                return Forbid();
            
            return BadRequest(new UpdateHeroResponse { Hero = null, Info = new[] { result.ErrorMessage }});
        }

        // for cyclic dependency
        result.Value.User = null;
        return Ok(new UpdateHeroResponse { Hero = result.Value, Info = new[] { SuccessMessages.Hero.Updated }});
    }

    [HttpPost, Route(ApiRoutes.Hero.Create)]
    public async Task<IActionResult> Create(CreateHeroRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        
        // we create a hero using a name. if new fields appear in CreateHeroRequest, we must add the value of these fields to the Hero object
        var hero = new Hero { Name = request.Name };
        var result = await _heroService.Create(userId, hero, cancellationToken);

        if (result.Success == false)
        {
            _logger.LogWarning("Can not create hero: " + result.ErrorMessage);
            return BadRequest(new CreateHeroResponse { HeroId = Guid.Empty, Info = new[] { result.ErrorMessage }});
        }

        // for cyclic dependency
        result.Value.User = null;
        var response = new CreateHeroResponse { HeroId = result.Value.HeroId, Info = new[] { SuccessMessages.Hero.Created }};
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
        var locationUrl = $"{baseUrl}/{nameof(HeroController).Replace("Controller", "")}/{ApiRoutes.Hero.GetById.Replace("{id:int}", response.HeroId.ToString())}";
        return Created(locationUrl, response);
    }

    [HttpGet, Route(ApiRoutes.Hero.GetById)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var hero = await _heroService.GetByIdAsync(id, cancellationToken);
        if (hero is null)
            return Ok(new GetHeroResponse { Hero = null, Info = new []{ErrorMessages.Hero.NotFound} });
        
        SolveCyclicDependency(hero);
        var heroMap = await _heroMapService.GetHeroMapAsync(id, cancellationToken);
        var response = new GetHeroResponse { Hero = hero, Map = heroMap, Info = new[] { SuccessMessages.Hero.Found } };
        return Ok(response);
    }
    
    private void SolveCyclicDependency(Hero heroToSolve)
    {
        // solve cyclic dependency
        heroToSolve.User = null;

        if (heroToSolve.Session != null) 
            heroToSolve.Session.Heroes = null;
    }

    private Guid GetUserId()
    {
        var result = Guid.Empty;
        string? userId = User?.FindFirst("id")?.Value;
        if (Guid.TryParse(userId, out result) == true)
        {
            return result;
        }
        else
        {
            _logger.LogError($"Can not resolve user id ({userId}), it's not guid type");
            throw new ArgumentException("Invalid Guid format");
        }
    }
}