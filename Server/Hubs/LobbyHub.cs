using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedLibrary.Contracts.Hubs;

namespace Server.Hubs;

public class LobbyHub : Hub
{
    private readonly ILogger<LobbyHub> _logger;
    public LobbyHub(ILogger<LobbyHub> logger)
    {
        _logger = logger;
    }

    [Authorize]
    public async Task HealthCheck()
    {
        await this.Clients.All.SendAsync(ClientHandlers.Lobby.HealthHandler, "online");
    }
}