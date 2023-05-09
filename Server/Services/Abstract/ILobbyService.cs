using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface ILobbyService
{
    /// <summary>
    /// Gets a list of all lobbies that have available slots for new players.
    /// </summary>
    /// <returns>A list of lobbies that have available slots.</returns>
    Task<List<Lobby>> GetAllLobbiesAsync();
    /// <summary>
    /// Gets a lobby with the given ID, including information about its users.
    /// </summary>
    /// <param name="id">The ID of the lobby to get.</param>
    /// <returns>The lobby with the given ID, or null if it doesn't exist.</returns>
    Task<Lobby?> GetLobbyByIdAsync(Guid id);
    /// <summary>
    /// Creates a new lobby with the given details.
    /// </summary>
    /// <param name="lobby">The details of the new lobby.</param>
    /// <returns>A service result containing the newly created lobby.</returns>
    Task<ServiceResult<Lobby>> CreateLobbyAsync(Lobby lobby);
    /// <summary>
    /// Deletes a lobby if it has no users.
    /// </summary>
    /// <param name="id">The ID of the lobby to delete.</param>
    /// <returns>A service result indicating whether the lobby was deleted.</returns>
    Task<ServiceResult<Guid>> DeleteLobbyIfThereAreNoUsers(Guid id);
    /// <summary>
    /// Connects a user to a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to connect.</param>
    /// <param name="lobbyId">The ID of the lobby to connect to.</param>
    /// <returns>A service result containing the lobby that the user was connected to.</returns>
    Task<ServiceResult<Lobby>> ConnectUserAsync(int userId, Guid lobbyId);
    /// <summary>
    /// Removes a user from a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to remove.</param>
    /// <param name="lobbyId">The ID of the lobby to remove the user from.</param>
    /// <returns>A service result containing the lobby that the user was removed from.</returns>
    Task<ServiceResult<Lobby>> ExitAsync(int userId, Guid lobbyId);
    /// <summary>
    /// Updates lobby fields
    /// </summary>
    /// <param name="lobby">The lobby with new data.</param>
    /// <returns>A service result containing the lobby with the updated user color.</returns>
    Task<ServiceResult<Lobby>> ChangeLobbyDataAsync(Lobby lobby);
    /// <summary>
    /// Changes the ready status of a user in a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to change the ready status of.</param>
    /// <param name="lobbyId">The ID of the lobby the user is in.</param>
    /// <returns>A service result containing the lobby with the updated ready status.</returns>
    Task<ServiceResult<LobbyInfo>> ChangeReadyStatusAsync(int userId, Guid lobbyId);
    /// <summary>
    /// Changes the color of a user in a lobby.
    /// </summary>
    /// <param name="userId">The ID of the user to change the ready status of.</param>
    /// <param name="lobbyId">The ID of the lobby the user is in.</param>
    /// <param name="argb">The new color in argb format</param>
    /// <returns>A service result containing the lobby with the updated user color.</returns>
    Task<ServiceResult<LobbyInfo>> ChangeColorAsync(int userId, Guid lobbyId, int argb);
}