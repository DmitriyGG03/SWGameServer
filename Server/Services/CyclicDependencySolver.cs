using SharedLibrary.Models;

namespace Server.Services;

public class CyclicDependencySolver
{
    public void Solve(Session sessionToSolve)
    {
        if (sessionToSolve.Heroes != null)
        {
            foreach (var item in sessionToSolve.Heroes)
            {
                item.User = null;
                item.Session = null;
            }
        }

        sessionToSolve.SessionMap = null;
    }
    
    public void Solve(LobbyInfo lobbyInfoToSolve)
    {
        lobbyInfoToSolve.Lobby = null;
        if (lobbyInfoToSolve.User is not null)
        {
            lobbyInfoToSolve.User.LobbyInfos = null;
            lobbyInfoToSolve.User.Heroes = null;
        }
    }
    
    public void Solve(Lobby lobbyToSolve)
    {
        var lobby = lobbyToSolve;
        if (lobby.LobbyInfos != null)
        {
            foreach (var item in lobby.LobbyInfos)
            {
                Solve(item);
            }
        }
    }

    public void Solve(IEnumerable<Lobby> lobbies)
    {
        foreach (var lobby in lobbies)
        {
            Solve(lobby);
        }
    }
}