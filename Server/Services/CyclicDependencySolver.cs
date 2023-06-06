using SharedLibrary.Models;

namespace Server.Services;

public class CyclicDependencySolver
{
    public void Solve(Hero hero)
    {
        if (hero.Session?.Heroes != null)
        {
            foreach (var sessionHero in hero.Session.Heroes)
            {
                sessionHero.Session = null;
                if (sessionHero.User != null) 
                    sessionHero.User.Heroes = null;
            }
        }

        if (hero.User != null) 
            hero.User.Heroes = null;
    }

    
    public void Solve(Battle battle)
    {
        if (battle.AttackerHero is not null)
        {
            battle.AttackerHero.Session = null;
            if (battle.AttackerHero.User is not null) 
                battle.AttackerHero.User.Heroes = null;
        }

        if (battle.DefendingHero is not null)
        {
            battle.DefendingHero.Session = null;
            if (battle.DefendingHero.User is not null) 
                battle.DefendingHero.User.Heroes = null;
        }
    }
    
    public void Solve(Session sessionToSolve)
    {
        if (sessionToSolve.Heroes is not null)
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