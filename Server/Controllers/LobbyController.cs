using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Services;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using SharedLibrary.Routes;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class LobbyController : ControllerBase
{
    private readonly ILobbyService _lobbyService;
    private readonly CyclicDependencySolver _cyclicDependencySolver;
    private readonly ILogger<LobbyService> _logger;
    
    public LobbyController(ILobbyService lobbyService, ILogger<LobbyService> logger, CyclicDependencySolver cyclicDependencySolver)
    {
        _lobbyService = lobbyService;
        _logger = logger;
        _cyclicDependencySolver = cyclicDependencySolver;
    }
    
    [HttpGet, Route(ApiRoutes.Lobby.GetAll)]
    public async Task<IActionResult> GetAllLobbies()
    {
        var lobbies = await _lobbyService.GetAllLobbiesAsync();

        if(lobbies.Any() == false) 
            return BadRequest(new GetAllLobbiesResponse(new[] { ErrorMessages.Lobby.NoLobbies }));

		_cyclicDependencySolver.Solve(lobbies);
        return Ok(new GetAllLobbiesResponse(new[] { SuccessMessages.Lobby.Found }, lobbies));
    }

    //Garbage endpoint
    
    //We no longer need getting lobby by Id
    
    // TODO: Remote it if there is no good reason to use it.
    #region Get lobby by id (garbage)
    [HttpGet, Route(ApiRoutes.Lobby.GetById)]
    public async Task<IActionResult> GetLobbyById([FromRoute] Guid id)
    {
        var lobby = await _lobbyService.GetLobbyByIdAsync(id);
        if (lobby is null)
            return NotFound();
        
        _cyclicDependencySolver.Solve(lobby);
        return Ok(new GetLobbyResponse { Lobby = lobby, Info = null });
    }
    #endregion
    
    [HttpPost, Route(ApiRoutes.Lobby.Create)]
    public async Task<IActionResult> CreateLobby(CreateLobbyRequest request)
    {
        var userId = GetUserId();

        var lobby = new Lobby { Id = Guid.NewGuid(), LobbyName = request.LobbyName, MaxHeroNumbers = request.MaxUsersCount };
        lobby.LobbyInfos = new List<LobbyInfo>
        {
            new LobbyInfo { Id = Guid.NewGuid(), LobbyLeader = true, Ready = true, LobbyId = lobby.Id, UserId = userId, ColorStatus = (int)ColorStatus.Red }
        };
        
        var result = await _lobbyService.CreateLobbyAsync(lobby);
        // solve cyclic dependency
        foreach (var item in result.Value.LobbyInfos)
        {
            item.Lobby = null;
            if (item.User is not null) 
                item.User.LobbyInfos = null;
        }
		var response = new CreateLobbyResponse { Lobby = result.Value, Info = new[] { SuccessMessages.Lobby.Created }};
		var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
        var locationUrl = $"{baseUrl}/{nameof(LobbyController).Replace("Controller", "")}/{ApiRoutes.Lobby.GetById.Replace("{id}", response.Lobby.Id.ToString())}";
        return Created(locationUrl, response);
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