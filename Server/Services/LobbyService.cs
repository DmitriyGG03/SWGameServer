using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public class LobbyService : ILobbyService
{
    private readonly GameDbContext _context;
    public LobbyService(GameDbContext context)
    {
        _context = context;
    }

    public async Task<List<Lobby>> GetAllLobbiesAsync()
    {
        return await _context.Lobbies.Include(x => x.LobbyInfos).Where(x => x.LobbyInfos.Count < x.MaxHeroNumbers).ToListAsync();
    }

    public Task<Lobby?> GetLobbyByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<Guid>> CreateLobbyAsync(Lobby lobby)
    {
        var exists = await _context.Lobbies.FirstOrDefaultAsync(x => x.LobbyName == lobby.LobbyName);
        if (exists is not null)
        {
            return new ServiceResult<Guid>(ErrorMessages.Lobby.SameName);
        }

        if (lobby.LobbyInfos is null)
        {
            throw new ArgumentException();
        }
        
        _context.Lobbies.Add(lobby);
        await _context.SaveChangesAsync();
        return new ServiceResult<Guid>(lobby.Id);
    }

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

    public async Task<ServiceResult<Lobby>> ConnectUserAsync(int userId, Guid lobbyId)
    {
        var lobby = await _context.Lobbies
            .Include(x => x.LobbyInfos)
            .ThenInclude(y => y.User)
            .FirstOrDefaultAsync(x => x.Id == lobbyId);
        
        if (lobby is null)
        {
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.NotFound);
        }
        
        if (lobby.LobbyInfos.Any(x => x.UserId == userId))
        {
            return new ServiceResult<Lobby>(ErrorMessages.Lobby.UserAlreadyInLobby);
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return new ServiceResult<Lobby>(ErrorMessages.User.NotFound);
        }

        var lobbyInfo = new LobbyInfo
        {
            Id = new Guid(),
            LobbyId = lobbyId,
            UserId = userId,
            User = user,
            LobbyLeader = false,
            Ready = false,
            Argb = Color.Red.ToArgb()
        };
        lobby.LobbyInfos.Add(lobbyInfo);
        await _context.SaveChangesAsync();

        return new ServiceResult<Lobby>(lobby);
    }
}