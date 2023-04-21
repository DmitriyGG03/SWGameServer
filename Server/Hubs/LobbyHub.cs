using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Services.Abstract;
using SharedLibrary.Contracts.Hubs;

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
        var userId = int.Parse( this.Context.User.FindFirst("id").Value);
        var result = await _lobbyService.ConnectUserAsync(userId, lobbyId);

        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.Lobby.ConnectionError, result.ErrorMessage);
        }

        var lobby = result.Value;
        foreach (var item in lobby.LobbyInfos)
        {
            item.Lobby = null;
            if (item.User != null)
            {
                item.User.LobbyInfos = null;
                item.User.Heroes = null;
            }
        }
        await this.Clients.All.SendAsync(ClientHandlers.Lobby.ConnectToLobbyHandler, lobby);
    }
}