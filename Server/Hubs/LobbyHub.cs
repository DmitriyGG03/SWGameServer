using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    private readonly ISessionService _sessionService;
    public LobbyHub(ILogger<LobbyHub> logger, ILobbyService lobbyService, GameDbContext context, ISessionService sessionService)
    {
        _logger = logger;
        _lobbyService = lobbyService;
        _sessionService = sessionService;
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

    [Authorize]
    public async Task ChangeLobbyData(Lobby lobby)
    {
        var result = await _lobbyService.ChangeLobbyDataAsync(lobby);
        await HandleResult(result, ClientHandlers.Lobby.ChangeLobbyDataHandler);
    }

    [Authorize]
    public async Task CreateSession(Lobby lobby)
    {
        var result = await _sessionService.CreateAsync(lobby.Id, CancellationToken.None);
        await HandleResult(result, ClientHandlers.Lobby.CreatedSessionHandler);
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
                return;
            }
            
            await this.Clients.Caller.SendAsync(ClientHandlers.Lobby.Error, result.ErrorMessage);
            return;
        }

        var lobby = SolveCyclicDependency(result.Value);
        await this.Clients.All.SendAsync(successMethod, lobby);
    }
    private async Task HandleResult(ServiceResult<Session> result, string successMethod)
    {
        if (result.Success == false)
        {
            if (result.ErrorMessage == SuccessMessages.Lobby.Deleted)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.Lobby.DeleteLobbyHandler, SuccessMessages.Lobby.Deleted);
                return;
            }
            
            await this.Clients.Caller.SendAsync(ClientHandlers.Lobby.Error, result.ErrorMessage);
            return;
        }

        var session = result.Value;
        // solve cyclic dependency
        session.SessionMap.Session = null;
        foreach (var item in session.Heroes)
        {
            item.Session = null;
            item.User = null;
            
            if (item.HeroMap?.Hero is not null)
            {
                item.HeroMap.Hero = null;
            }
        }
        
        await this.Clients.All.SendAsync(successMethod, session);
    }

    private Lobby SolveCyclicDependency(Lobby lobbyToSolve)
    {
        var lobby = lobbyToSolve;
        // for cyclic dependency
        if (lobby.LobbyInfos != null)
        {
            foreach (var item in lobby.LobbyInfos)
            {
                item.Lobby = null;
                if (item.User != null)
                {
                    item.User.LobbyInfos = null;
                    item.User.Heroes = null;
                }
            }
        }

        return lobby;
    }
}