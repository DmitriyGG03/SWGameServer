using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
	[HttpPost("register")]
	public IActionResult Register()
	{

	}

	[HttpPost("login")]
	public IActionResult Login()
	{

	}
}
