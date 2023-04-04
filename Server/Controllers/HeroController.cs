using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Services;
using SharedLibrary.Models;
using SharedLibrary.Requests;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class HeroController : ControllerBase
{
    public IHeroService HeroService { get; init; }
    public GameDbContext DbContext { get; init; }

    public HeroController(IHeroService playerService, GameDbContext dbContext)
    {
        HeroService = playerService;
		DbContext = dbContext;

        User user = new User() { 
            Username = "Dmitriy", 
            PasswordHash = "password69", 
            Salt = "gdfbrxtbxrt"
        };

		DbContext.Add(user);

        DbContext.SaveChanges();
	}

    [HttpPost("{id}")]
    public IActionResult Edit([FromRoute] int id, [FromBody] CreateHeroRequest request)
    {
        var availableHeroId = JsonConvert.DeserializeObject<int>(User.FindFirst("hero").Value);

        if(!availableHeroId.Equals(id)) return Unauthorized();

        var hero = DbContext.Heroes.First(h => h.HeroId.Equals(id));

        hero.Name = request.Name;

        DbContext.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public Hero Post(CreateHeroRequest request)
    {
        var userId = int.Parse(User.FindFirst("id").Value);
        var user = DbContext.Users.First(u => u.Id == userId);

        var hero = new Hero()
        {
            Name = request.Name,
            Resourses = 0, //Must be changed later
            ResearchShipLimit = 0, //Must be too
            ColonizationShipLimit = 0, //Must be too

            User = user,
            //Add Planet by control
        };

        DbContext.Add(hero);
        DbContext.SaveChanges();

        return hero;
	}
}