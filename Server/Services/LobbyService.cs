using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public class LobbyService : ILobbyService
{
    public Task<List<Lobby>> GetAllLobbiesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Lobby?> GetLobbyByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<Guid>> CreateLobbyAsync(Lobby lobby)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<Guid>> DeleteLobbyIfThereAreNoUsers(Guid id)
    {
        throw new NotImplementedException();
    }
}