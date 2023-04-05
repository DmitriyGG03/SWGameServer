using Azure.Identity;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Mvc;
using Server.Services;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
	public IAuthenticationService AuthService { get; init; }

	public AuthenticationController(IAuthenticationService authService)
	{
		AuthService = authService;
	}

	[HttpPost("register")]
	public IActionResult Register(AuthenticationRequest authRequest)
	{
		var (success, content) = AuthService.Register(authRequest.Username, authRequest.Email, authRequest.Password);
		if (!success) return BadRequest(content);

		return Login(authRequest);
	}

	[HttpPost("login")]
	public IActionResult Login(AuthenticationRequest authRequest)
	{
		var (success, content) = AuthService.Login(authRequest.Username, authRequest.Password);
		if (!success) return BadRequest(content);

		return Ok(new AuthenticationResponse() { Token = content });
	}
}
