using Microsoft.AspNetCore.Mvc;
using Server.Services;
using SharedLibrary;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerController : ControllerBase
{
    public IPlayerService PlayerService { get; init; }
    public GameDbContext DbContext { get; init; }

    public PlayerController(IPlayerService playerService, GameDbContext dbContext)
    {
        PlayerService = playerService;
		DbContext = dbContext;

        User user = new User() { 
            Username = "Dmitriy", 
            PasswordHash = "password69", 
            Salt = "gdfbrxtbxrt"
        };

		DbContext.Add(user);

        DbContext.SaveChanges();
	}

    [HttpGet("{id}")]
    public Player Get([FromRoute] int id)
    {
        var player = new Player() {Id = id};

        PlayerService.DoSomething();

        return player;
    }

    [HttpPost]
    public Player Post(Player player)
    {
        Console.WriteLine("Player has been added to the DB");
        return player;
    }
}