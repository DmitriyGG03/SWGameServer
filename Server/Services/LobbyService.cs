using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Common.Semaphores;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Services;

/// <summary>
/// A service for managing game lobbies.
/// </summary>
public class LobbyService : ILobbyService
{
    /// <summary>
    /// The database context for the game.
    /// </summary>
    private readonly GameDbContext _context;
    
    /// <summary>
    /// A logger for the lobby service.
    /// </summary>
    private readonly ILogger<LobbyService> _logger;
    
    /// <summary>
    /// Creates a new instance of the lobby service.
    /// </summary>
    /// <param name="context">The database context for the game.</param>
    /// <param name="logger">A logger for the lobby service.</param>
    public LobbyService(GameDbContext context, ILogger<LobbyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets a list of all lobbies that have available slots for new players.
    /// </summary>
    /// <returns>A list of lobbies that have available slots.</returns>
    public async Task<List<Lobby>> GetAllLobbiesAsync()
    {
        return await _context.Lobbies.Include(x => x.LobbyInfos).Where(x => x.LobbyInfos.Count < x.MaxHeroNumbers).ToListAsync();
    }

    /// <summary>
    /// Gets a lobby with the given ID, including information about its users.
    /// </summary>
    /// <param name="id">The ID of the lobby to get.</param>
    /// <returns>The lobby with the given ID, or null if it doesn't exist.</returns>
    public Task<Lobby?> GetLobbyByIdAsync(Guid id)
    {
        var result =  _context.Lobbies
            .Include(x => x.LobbyInfos)!
             .ThenInclude(i => i.User)
            .FirstOrDefaultAsync(l => l.Id == id);
        return result;
    }

    /// <summary>
    /// Creates a new lobby with the given details.
    /// </summary>
    /// <param name="lobby">The details of the new lobby.</param>
    /// <returns>A service result containing the newly created lobby.</returns>
    public async Task<ServiceResult<Lobby>> CreateLobbyAsync(Lobby lobby)
    {
        if (lobby.LobbyInfos is null || lobby.LobbyInfos.Any() == false)
            throw new ArgumentException("Oops, for some reason, the user was not added to the lobby");

        _context.Lobbies.Add(lobby);
        await _context.SaveChangesAsync();

        var userId = lobby.LobbyInfos.First().UserId;
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        lobby.LobbyInfos.First().User = user;

        return new ServiceResult<Lobby>(lobby);
    }

    #region Delete lobby if there are no users (garbage)
    
    /// <summary>
    /// Deletes a lobby if it has no users.
    /// </summary>
    /// <param name="id">The ID of the lobby to delete.</param>
    /// <returns>A service result indicating whether the lobby was deleted.</returns>
    public async Task<ServiceResult<Guid>> DeleteLobbyIfThereAreNoUsers(Guid id)
    {
        var exists = await _context.Lobbies.Include(x => x.LobbyInfos).FirstOrDefaultAsync(x => x.Id == id);
        if (exists is null)
        {
            return new ServiceResult<Guid>(ErrorMessages.Lobby.NotFound);
        }

        if (exists.LobbyInfos.Any())
        {
            return new ServiceResult<Guid>(ErrorMessages.Lobby.ThereIsUsers);
        }

        _context.Lobbies.Remove(exists);
        await _context.SaveChangesAsync();
        return new ServiceResult<Guid>(id);
    }
    #endregion

    /// <summary>
    /// Connects a user to a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to connect.</param>
    /// <param name="lobbyId">The ID of the lobby to connect to.</param>
    /// <returns>A service result containing the lobby that the user was connected to.</returns>
    public async Task<ServiceResult<Lobby>> ConnectUserAsync(Guid userId, Guid lobbyId)
    {
        var lobby = await GetLobbyWithInfosByIdAsync(lobbyId);
        if (lobby is null)
        {
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.NotFound);
        }
        
        if (IsUserInLobby(userId, lobby) == true)
        {
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.UserAlreadyInLobby);
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return new ServiceResult<Lobby>(ErrorMessages.User.NotFound);
        }

        return await AddUserInLobbyAsync(user, lobby);
    }

    /// <summary>
    /// Removes a user from a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <param name="lobbyId">The ID of the lobby to remove the user from.</param>
    /// <returns>A service result containing the lobby that the user was removed from.</returns>
    public async Task<ServiceResult<Lobby>> ExitAsync(Guid userId, Guid lobbyId)
    {
        var lobby = await GetLobbyWithInfosByIdAsync(lobbyId);
        if (lobby is null)
        {
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.NotFound);
        }
        
        if (IsUserInLobby(userId, lobby) == false)
        {
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.UserIsNotInLobby);
        }

        var userInfo = lobby.LobbyInfos.FirstOrDefault(x => x.UserId == userId) ?? throw new NullReferenceException();
        if (userInfo.LobbyLeader == true)
        {
            return await DeleteLobbyAsync(lobby, userInfo);
        }
        return await ExitFromLobbyAsync(lobby, userInfo);
    }

    /// <summary>
    /// Changes the ready status of a user in a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to change the ready status of.</param>
    /// <param name="lobbyId">The ID of the lobby the user is in.</param>
    /// <returns>A service result containing the lobby with the updated ready status.</returns>
    public async Task<ServiceResult<LobbyInfo>> ChangeReadyStatusAsync(Guid userId, Guid lobbyId)
    {
        var result = await GetLobbyAndValidateIfUserThere(userId, lobbyId);
        if (result.Success == false)
            return new ServiceResult<LobbyInfo>(result.ErrorMessage);
        var lobby = result.Value;

        var lobbyInfo = lobby.LobbyInfos.First(x => x.UserId == userId);
        lobbyInfo.Ready = !lobbyInfo.Ready;
        await _context.SaveChangesAsync();
        return new ServiceResult<LobbyInfo>(lobbyInfo);
    }
    
    /// <summary>
    /// Changes the color of a user in a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to change the ready status of.</param>
    /// <param name="lobbyId">The ID of the lobby the user is in.</param>
    /// <param name="colorStatus">The new color in argb format</param>
    /// <returns>A service result containing the lobby with the updated user color.</returns>
    public async Task<ServiceResult<LobbyInfo>> ChangeColorAsync(Guid userId, Guid lobbyId, int colorStatus)
    {
        var result = await GetLobbyAndValidateIfUserThere(userId, lobbyId);
        if (result.Success == false)
            return new ServiceResult<LobbyInfo>(result.ErrorMessage);
        var lobby = result.Value;
        
        var lobbyInfo = lobby.LobbyInfos.First(x => x.UserId == userId);
        lobbyInfo.ColorStatus = (ColorStatus)colorStatus;
        
        await _context.SaveChangesAsync();
        return new ServiceResult<LobbyInfo>(lobbyInfo);
    }
    
    /// <summary>
    /// Updates lobby fields
    /// </summary>
    /// <param name="lobby">The lobby with new data.</param>
    /// <returns>A service result containing the lobby with the updated user color.</returns>
    public async Task<ServiceResult<Lobby>> ChangeLobbyDataAsync(Lobby lobby)
    {
        var result = _context.Lobbies.Update(lobby);
        await _context.SaveChangesAsync();
        return new ServiceResult<Lobby>(lobby);
    }

    private async Task<ServiceResult<Lobby>> GetLobbyAndValidateIfUserThere(Guid userId, Guid lobbyId)
    {
        var lobby = await GetLobbyByIdAsync(lobbyId);
        if (lobby is null)
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.NotFound);

        var userInLobby = IsUserInLobby(userId, lobby);
        if (userInLobby == false)
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.UserIsNotInLobby);

        return new ServiceResult<Lobby>(lobby);
    }
    private bool IsUserInLobby(Guid userId, Lobby lobby)
    {
        return lobby.LobbyInfos.Any(x => x.UserId == userId);
    }
    private async Task<ServiceResult<Lobby>> AddUserInLobbyAsync(ApplicationUser user, Lobby lobby)
    {
        var lobbyInfo = new LobbyInfo
        {
            Id = new Guid(),
            LobbyId = lobby.Id,
            UserId = user.Id,
            User = user,
            LobbyLeader = false,
            Ready = false,
            ColorStatus = (int)ColorStatus.Red
        };
        
        lobby.LobbyInfos.Add(lobbyInfo);
        await _context.SaveChangesAsync();
        return new ServiceResult<Lobby>(lobby);
    }
    private async Task<ServiceResult<Lobby>> DeleteLobbyAsync(Lobby lobby, LobbyInfo userInfo)
    {
        _context.Lobbies.Remove(lobby);
        await _context.SaveChangesAsync();
        return new ServiceResult<Lobby>(SuccessMessages.Lobby.Deleted);
    }
    private async Task<ServiceResult<Lobby>> ExitFromLobbyAsync(Lobby lobby, LobbyInfo userInfo)
    {
        lobby.LobbyInfos.Remove(userInfo);
        await _context.SaveChangesAsync();
        return new ServiceResult<Lobby>(lobby);
    }
    private async Task<Lobby?> GetLobbyWithInfosByIdAsync(Guid lobbyId)
    {
        return await _context.Lobbies
            .Include(x => x.LobbyInfos)
             .ThenInclude(y => y.User)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);
    }
}