using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Common.Semaphores;
using Server.Domain;
using Server.Services;
using Server.Services.Abstract;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;

namespace Server.Hubs;

public class LobbyHub : Hub
{
    private readonly ILogger<LobbyHub> _logger;
    private readonly ILobbyService _lobbyService;
    private readonly ISessionService _sessionService;
    private readonly CyclicDependencySolver _cyclicDependencySolver;
    public LobbyHub(ILogger<LobbyHub> logger, ILobbyService lobbyService, GameDbContext context, ISessionService sessionService, CyclicDependencySolver cyclicDependencySolver)
    {
        _logger = logger;
        _lobbyService = lobbyService;
        _sessionService = sessionService;
        _cyclicDependencySolver = cyclicDependencySolver;
    }

    [Authorize]
    public async Task HealthCheck()
    {
        _logger.LogInformation($"{nameof(HealthCheck)}: health check invoked");
        await this.Clients.All.SendAsync(ClientHandlers.Lobby.HealthHandler, "online");
    }

    [Authorize]
    public async Task ConnectToLobby(Guid lobbyId, string name)
    {
        var userId = GetUserIdFromContext();
        var result = await _lobbyService.ConnectUserAsync(userId, lobbyId, name);
        await HandleResult(result, ClientHandlers.Lobby.ConnectToLobbyHandler);
    }
    [Authorize]
    public async Task ExitFromLobby(Guid lobbyId)
    {
        var userId = GetUserIdFromContext();
        var result = await _lobbyService.ExitAsync(userId, lobbyId);
        await HandleResult(result, ClientHandlers.Lobby.ExitFromLobbyHandler);
    }

    #region Is it still needed?

    [Authorize]
    public async Task ChangeLobbyData(Lobby lobby)
    {
        var result = await _lobbyService.ChangeLobbyDataAsync(lobby);
        await HandleResult(result, ClientHandlers.Lobby.ChangeLobbyDataHandler);
    }

    #endregion
    
    [Authorize]
    public async Task ChangeReadyStatus(Guid lobbyId)
    {
        var semaphore = ApplicationSemaphores.SemaphoreSlimForChangingReadyStatus;
        await semaphore.WaitAsync();

        try
        {
            var userId = GetUserIdFromContext();
            var result = await _lobbyService.ChangeReadyStatusAsync(userId, lobbyId);
            await HandleResult(result, ClientHandlers.Lobby.ChangeReadyStatus);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error caused by changing ready status operation");
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }
    [Authorize]
    public async Task ChangeColor(Guid lobbyId, int colorStatus)
    {
        var semaphore = ApplicationSemaphores.SemaphoreSlimForChangingColor;
        await semaphore.WaitAsync();

        try
        {
            var userId = GetUserIdFromContext();
            var result = await _lobbyService.ChangeColorAsync(userId, lobbyId, colorStatus);
            await HandleResult(result, ClientHandlers.Lobby.ChangedColor);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error caused by changing color operation");
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }
    
    [Authorize]
    public async Task CreateSession(Lobby lobby)
    {
        var result = await _sessionService.CreateAsync(lobby.Id, CancellationToken.None);
        await HandleResult(result, ClientHandlers.Lobby.CreatedSessionHandler);
    }

    private Guid GetUserIdFromContext()
    {
        var result = Guid.Empty;
        string? userId = this.Context.User?.FindFirst("id")?.Value;
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
    private async Task HandleResult(ServiceResult<Lobby> result, string successMethod)
    {
        if((await ValidateResultIfInvalidSendMessageToCallerAsync(result)) == false)
            return;

        var lobby = result.Value;
        _cyclicDependencySolver.Solve(lobby);
        await this.Clients.All.SendAsync(successMethod, lobby);
    }
    private async Task HandleResult(ServiceResult<LobbyInfo> result, string successMethod)
    {
        if((await ValidateResultIfInvalidSendMessageToCallerAsync(result)) == false)
            return;

        var lobbyInfo = result.Value;
        _cyclicDependencySolver.Solve(lobbyInfo);
        await this.Clients.All.SendAsync(successMethod, result.Value);
    }
    private async Task HandleResult(ServiceResult<Session> result, string successMethod)
    {
        if((await ValidateResultIfInvalidSendMessageToCallerAsync(result)) == false)
            return;

        await this.Clients.All.SendAsync(successMethod, result.Value.Id);
    }

    private async Task<bool> ValidateResultIfInvalidSendMessageToCallerAsync<T>(ServiceResult<T> result)
    {
        if (result.Success == false)
        {
            if (result.ErrorMessage == SuccessMessages.Lobby.Deleted)
            {
                await this.Clients.All.SendAsync(ClientHandlers.Lobby.DeleteLobbyHandler, SuccessMessages.Lobby.Deleted);
                return false;
            }
            
            await this.Clients.Caller.SendAsync(ClientHandlers.Lobby.Error, result.ErrorMessage);
            return false;
        }

        return true;
    }
}