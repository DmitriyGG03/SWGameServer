using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;

namespace Server.Hubs;

public class LobbyHub : Hub
{
    private readonly ILogger<LobbyHub> _logger;
    private readonly ILobbyService _lobbyService;
    public LobbyHub(ILogger<LobbyHub> logger, ILobbyService lobbyService)
    {
        _logger = logger;
        _lobbyService = lobbyService;
    }

    [Authorize]
    public async Task HealthCheck()
    {
        _logger.LogInformation($"{nameof(HealthCheck)}: health check invoked");
        await this.Clients.All.SendAsync(ClientHandlers.Lobby.HealthHandler, "online");
    }

    [Authorize]
    public async Task ConnectToLobby(Guid lobbyId)
    {
        var userId = GetUserIdFromContext();
        var result = await _lobbyService.ConnectUserAsync(userId, lobbyId);
        await HandleResult(result, ClientHandlers.Lobby.ConnectToLobbyHandler);
    }
    [Authorize]
    public async Task ExitFromLobby(Guid lobbyId)
    {
        var userId = GetUserIdFromContext();
        var result = await _lobbyService.ExitAsync(userId, lobbyId);
        await HandleResult(result, ClientHandlers.Lobby.ExitFromLobbyHandler);
    }

    private int GetUserIdFromContext()
    {
        return int.Parse(this.Context.User.FindFirst("id").Value);
    }
    private async Task HandleResult(ServiceResult<Lobby> result, string successMethod)
    {
        if (result.Success == false)
        {
            if (result.ErrorMessage == SuccessMessages.Lobby.Deleted)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.Lobby.DeleteLobbyHandler, SuccessMessages.Lobby.Deleted);
            }
            
            await this.Clients.Caller.SendAsync(ClientHandlers.Lobby.Error, result.ErrorMessage);
        }

        var lobby = SolveCyclicDependency(result.Value);
        await this.Clients.All.SendAsync(successMethod, lobby);
    }
    private Lobby SolveCyclicDependency(Lobby lobbyToSolve)
    {
        var lobby = lobbyToSolve;
        // for cyclic dependency
        foreach (var item in lobby.LobbyInfos)
        {
            item.Lobby = null;
            if (item.User != null)
            {
                item.User.LobbyInfos = null;
                item.User.Heroes = null;
            }
        }

        return lobby;
    }
}