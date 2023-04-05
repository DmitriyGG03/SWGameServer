using Microsoft.AspNetCore.Mvc;
using Server.Services.Abstract;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using SharedLibrary.Routes;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
	private readonly IAuthenticationService _authenticationService;

	public AuthenticationController(IAuthenticationService authService)
	{
		_authenticationService = authService;
	}

	[HttpPost(ApiRoutes.Authentication.Register)]
	public IActionResult Register([FromBody] RegistrationRequest authRequest)
	{
		var (success, content) = _authenticationService.Register(authRequest.Username, authRequest.Email, authRequest.Password);
		return ValidateServiceResultAndReturnResponse((success, content));
	}
	[HttpPost(ApiRoutes.Authentication.Login)]
	public IActionResult Login([FromBody] LoginRequest request)
	{
		var (success, content) = _authenticationService.Login(request.Email, request.Password);
        return ValidateServiceResultAndReturnResponse((success, content));
    }

    private IActionResult ValidateServiceResultAndReturnResponse((bool, string) result)
	{
		bool success = result.Item1;
		string content = result.Item2;

        if (!success) 
			return BadRequest(new AuthenticationFailedResponse(new string[] { content }));
        return Ok(new AuthenticationResponse() { Token = content });
    }
}
