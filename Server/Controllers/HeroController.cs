using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Services;
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

    public HeroController(IHeroService heroService, ILogger<HeroController> logger)
    {
        _heroService = heroService;
        _logger = logger;
    }

    [HttpPut, Route(ApiRoutes.Hero.Update)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CreateHeroRequest request, CancellationToken cancellationToken)
    {
        /* this operation is not working
         * var availableHeroId = JsonConvert.DeserializeObject<int>(User.FindFirst("hero").Value);
            if(!availableHeroId.Equals(id)) 
            return Unauthorized();
         */
        var userId = int.Parse(User.FindFirst("id").Value);
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
        var userId = int.Parse(User.FindFirst("id").Value);
        
        // we create a hero using a name. if new fields appear in CreateHeroRequest, we must add the value of these fields to the Hero object
        var hero = new Hero { Name = request.Name };
        var result = await _heroService.Create(userId, hero, cancellationToken);

        if (result.Success == false)
        {
            _logger.LogWarning("Can not create hero: " + result.ErrorMessage);
            return BadRequest(new CreateHeroResponse { HeroId = -1, Info = new[] { result.ErrorMessage }});
        }

        // for cyclic dependency
        result.Value.User = null;
        var response = new CreateHeroResponse { HeroId = result.Value.HeroId, Info = new[] { SuccessMessages.Hero.Created }};
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
        var locationUrl = $"{baseUrl}/{nameof(HeroController).Replace("Controller", "")}/{ApiRoutes.Hero.GetById.Replace("{id:int}", response.HeroId.ToString())}";
        return Created(locationUrl, response);
    }

    [HttpGet, Route(ApiRoutes.Hero.GetById)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var hero = await _heroService.GetByIdAsync(id, cancellationToken);
        if (hero is null)
            return NotFound();
        
        return Ok(new GetHeroResponse { Hero = hero, Info = null });
    }
}