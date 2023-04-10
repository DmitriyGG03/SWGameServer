using Microsoft.AspNetCore.Mvc;
using Server.Domain;
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
		var result = _authenticationService.Register(authRequest.Username, authRequest.Email, authRequest.Password);
		return ValidateServiceResultAndReturnResponse(result);
	}
	[HttpPost(ApiRoutes.Authentication.Login)]
	public IActionResult Login([FromBody] LoginRequest request)
	{
        var result = _authenticationService.Login(request.Email, request.Password);
        return ValidateServiceResultAndReturnResponse(result);
    }

    private IActionResult ValidateServiceResultAndReturnResponse(AuthenticationResult result)
	{
        if (result.Success == false) 
			return BadRequest(new AuthenticationResponse(result.OperationInfo));

        return Ok(new AuthenticationResponse(result.OperationInfo, result.AccessToken));
    }
}
