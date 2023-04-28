using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Common.Constants;
using Server.Services.Abstract;
using SharedLibrary.Models;
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
    public LobbyController(ILobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }
    
    [HttpGet, Route(ApiRoutes.Lobby.GetAll)]
    public async Task<IActionResult> GetAllLobbiesAsync()
    {
        var lobbies = await _lobbyService.GetAllLobbiesAsync();

        if(lobbies is null) BadRequest(new GetAllLobbiesResponse(new[] { "No active lobbies found. You can start a new game" }));

		// For cyclic dependency
		foreach (var lobby in lobbies)
        {
            foreach (var info in lobby.LobbyInfos)
            {
                info.Lobby = null;
            }
        }
        
        return Ok(new GetAllLobbiesResponse(new[] { "Lobbies has been successfully found" }, lobbies));
    }

    //Garbage endpoint
    
    //We no longer need getting lobby by Id
    
    // TODO: Remote it if there is no good reason to use it.
    #region Get lobby by id (garbage)
    [HttpGet, Route(ApiRoutes.Lobby.GetById)]
    public async Task<IActionResult> GetLobbyByIdAsync([FromRoute] Guid id)
    {
        var lobby = await _lobbyService.GetLobbyByIdAsync(id);
        if (lobby is null)
            return NotFound();
        // For cyclic dependency
        foreach (var info in lobby.LobbyInfos)
        {
            info.Lobby = null;
        }
        return Ok(new GetLobbyResponse { Lobby = lobby, Info = null });
    }
    #endregion
    
    [HttpPost, Route(ApiRoutes.Lobby.Create)]
    public async Task<IActionResult> CreateLobbyAsync(CreateLobbyRequest request)
    {
        var userId = int.Parse(User.FindFirst("id").Value);

        var lobby = new Lobby { Id = Guid.NewGuid(), LobbyName = request.LobbyName, MaxHeroNumbers = request.MaxUsersCount };
        lobby.LobbyInfos = new List<LobbyInfo>
        {
            new LobbyInfo { Id = Guid.NewGuid(), LobbyLeader = true, Ready = false, LobbyId = lobby.Id, UserId = userId, Argb = Color.Red.ToArgb() }
        };
        
        var result = await _lobbyService.CreateLobbyAsync(lobby);
        // solve cyclic dependency
        foreach (var item in result.Value.LobbyInfos)
        {
            item.Lobby = null;
        }
		var response = new CreateLobbyResponse { Lobby = result.Value, Info = new[] { SuccessMessages.Lobby.Created }};
		var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
        var locationUrl = $"{baseUrl}/{nameof(LobbyController).Replace("Controller", "")}/{ApiRoutes.Lobby.GetById.Replace("{id}", response.Lobby.Id.ToString())}";
        return Created(locationUrl, response);
    }
}