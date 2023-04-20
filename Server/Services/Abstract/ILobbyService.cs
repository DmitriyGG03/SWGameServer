using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface ILobbyService
{
    Task<List<Lobby>> GetAllLobbiesAsync();
    Task<Lobby?> GetLobbyByIdAsync(Guid id);
    Task<ServiceResult<Guid>> CreateLobbyAsync(Lobby lobby);
    Task<ServiceResult<Guid>> DeleteLobbyIfThereAreNoUsers(Guid id);
}