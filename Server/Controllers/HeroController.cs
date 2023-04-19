using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Domain;
using Server.Services;
using SharedLibrary.Models;
using SharedLibrary.Requests;

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

    [HttpPut, Route("{id:int}")]
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
        
        return ValidateResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateHeroRequest request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst("id").Value);
        
        // we create a hero using a name. if new fields appear in CreateHeroRequest, we must add the value of these fields to the Hero object
        var hero = new Hero { Name = request.Name };
        var result = await _heroService.Create(userId, hero, cancellationToken);

        return ValidateResult(result);
    }

    [HttpGet, Route("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var hero = await _heroService.GetByIdAsync(id, cancellationToken);
        if (hero is null)
            return NotFound();
        
        return Ok(hero);
    }

    private IActionResult ValidateResult(ServiceResult<Hero> result)
    {
        if (result.Success == false)
        {
            if (result.ErrorMessage == ErrorMessages.UserHasNoAccessToGivenHero)
                return Forbid();
            
            // for now it throws exception, because there is no validation. if they will appear we have to create new response
            _logger.LogError("Can not update hero: " + result.ErrorMessage);
            throw new Exception(result.ErrorMessage);
        }

        // for cyclic dependency
        result.Value.User = null;
        return Ok(result.Value);
    }
}