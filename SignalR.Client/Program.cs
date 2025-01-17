﻿using System.Drawing;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using SignalR.Client;
using JsonSerializer = System.Text.Json.JsonSerializer;

Settings? settings = null;

using (var r = new StreamReader(@"D:\Asp .Net Core\Projects\SWGameServer\SignalR.Client\settings.json"))
{
    string json = r.ReadToEnd();
    settings = JsonConvert.DeserializeObject<Settings>(json);

    if (settings is null)
        throw new InvalidOperationException();
}

if (settings is null)
    throw new InvalidOperationException();

const int port = 7148;
Console.WriteLine("Enter hub name");
string hubName = Console.ReadLine();

Console.WriteLine("Enter access token: ");
var accessToken = Console.ReadLine();

var lobbyId = Guid.Empty;
if (hubName == "lobby")
{
    lobbyId = settings.LobbyId;
}

Guid sessionId = Guid.Empty, hero1 = Guid.Empty, hero2 = Guid.Empty;
if (hubName == "session")
{
    sessionId = settings.Session.Id;
    var heroes = settings.Session.Heroes.OrderBy(x => x.Name);
    hero1 = heroes.First().HeroId;
    hero2 = heroes.FirstOrDefault(x => x.HeroId != hero1).HeroId;
}

try
{
    Lobby? currentLobby = null;
    
    var connection = new HubConnectionBuilder()
        .WithUrl($"https://localhost:{port}/hubs/{hubName}", options =>
        { 
            options.AccessTokenProvider = () => Task.FromResult(accessToken);
        })
        .WithAutomaticReconnect()
        .Build();
    
    currentLobby = ConfigureHandlers(connection);
    await connection.StartAsync();
    
    while (true)
    {
        Console.WriteLine("Type message to execute operation: ");
        var message = Console.ReadLine();
        
        var result = await ParseMessageAndSendRequestToServerAsync(message, connection, new Lobby { Id = lobbyId });
        if(result == true)
            break;

        Console.ReadKey();
    }
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    throw;
}

Console.ReadKey();

Lobby? ConfigureHandlers(HubConnection hubConnection)
{
    Lobby? currentLobby1 = null;
    
    hubConnection.On<string>(ClientHandlers.Lobby.Error, (errorMessage) =>
    {
        var message = $"Server error: {errorMessage}";
        Console.WriteLine(message);
    });
    
    hubConnection.On<string>(ClientHandlers.Lobby.DeleteLobbyHandler, (serverMessage) =>
    {
        var message = $"Server message: {serverMessage}";
        Console.WriteLine(message);
    });
    
    hubConnection.On<Lobby>(ClientHandlers.Lobby.ConnectToLobbyHandler, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }

        currentLobby1 = lobby;
    });

    hubConnection.On<Lobby>(ClientHandlers.Lobby.ExitFromLobbyHandler, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }

        currentLobby1 = lobby;
    });
    
    hubConnection.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }

        currentLobby1 = lobby;
    });
    
    hubConnection.On<LobbyInfo>(ClientHandlers.Lobby.ChangedColor, (info) =>
    {
        Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready + "; " + info.ColorStatus);
    });
    
    hubConnection.On<LobbyInfo>(ClientHandlers.Lobby.ChangeReadyStatus, (info) =>
    {
        Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready + "; " + info.ColorStatus);
    });
    
    hubConnection.On<Guid>(ClientHandlers.Lobby.CreatedSessionHandler, (sessionId) =>
    {
        Console.WriteLine("Session id: " + sessionId);
    });

    hubConnection.On<HeroMapView>(ClientHandlers.Session.ReceiveHeroMap, (heroMap) =>
    {
        Console.WriteLine("Received hero map");
        string json = JsonSerializer.Serialize(heroMap, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<Session>(ClientHandlers.Session.ReceiveSession, (session) =>
    {
        Console.WriteLine("Received session");
        string json = JsonSerializer.Serialize(session, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<UpdatedFortificationResponse>(ClientHandlers.Session.UpdatedFortification, (fortificationResponse) =>
    {
        Console.WriteLine("UpdatedFortificationResponse");
        string json = JsonSerializer.Serialize(fortificationResponse, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<Battle>(ClientHandlers.Session.ReceiveBattle, (battle) =>
    {
        Console.WriteLine("Battle");
        string json = JsonSerializer.Serialize(battle, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<NextTurnResponse>(ClientHandlers.Session.NextTurnHandler, (nextTurn) =>
    {
        Console.WriteLine("NextTurnResponse");
        string json = JsonSerializer.Serialize(nextTurn, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<UpdatedPlanetStatusResponse>(ClientHandlers.Session.StartPlanetResearchingOrColonization, (newPlanetStatus) =>
    {
        Console.WriteLine("UpdatedPlanetStatusResponse");
        string json = JsonSerializer.Serialize(newPlanetStatus, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<NextTurnResponse>(ClientHandlers.Session.GetHeroDataHandler, (nextTurnResponse) =>
    {
        Console.WriteLine("GetHeroData");
        string json = JsonSerializer.Serialize(nextTurnResponse, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<GameEndedResponse>(ClientHandlers.Session.GameEnded, (gameEndedResponse) =>
    {
        Console.WriteLine("GameEndedResponse");
        string json = JsonSerializer.Serialize(gameEndedResponse, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
        
    hubConnection.On<ExitFromSessionResponse>(ClientHandlers.Session.ExitFromSessionHandler, (response) =>
    {
        Console.WriteLine("ExitFromSessionResponse");
        string json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    });
    
    hubConnection.On<string>(ClientHandlers.ErrorHandler, HandleStringMessageOutput());
    hubConnection.On<string>(ClientHandlers.Session.PostResearchOrColonizeErrorHandler, HandleStringMessageOutput());
    hubConnection.On<string>(ClientHandlers.Session.HealthCheckHandler, HandleStringMessageOutput());

    return currentLobby1;
}
async Task<bool> ParseMessageAndSendRequestToServerAsync(string message, HubConnection connection, Lobby? currentLobby)
{
    if(message == "exit")
        return true;

    if (message == "connect")
    {
        await connection.InvokeAsync(ServerHandlers.Lobby.ConnectToLobby, lobbyId, Guid.NewGuid().ToString());
    }
    else if (message == "lobbyexit")
    {
        await connection.InvokeAsync(ServerHandlers.Lobby.ExitFromLobby, lobbyId);
    }
    else if (message == "change")
    {
        var lobbys = new Lobby();
        lobbys.LobbyName = "New Lobby";
        lobbys.MaxHeroNumbers = 10;
        lobbys.Id = lobbyId;
        await connection.InvokeAsync(ServerHandlers.Lobby.ChangeLobbyData, lobbys);
    }
    else if (message == "create session")
    {
        await connection.InvokeAsync(ServerHandlers.Lobby.CreateSession, new Lobby {Id = lobbyId });
    }
    else if (message == "ready status")
    {
        if (currentLobby is not null)
        {
            await connection.InvokeAsync(ServerHandlers.Lobby.ChangeReadyStatus, currentLobby.Id);
        }
        else
        {
            Console.WriteLine("Current lobby is null");
        }
    }
    else if (message == "change color")
    {
        // random color getting 
        var colors = new List<Color> { Color.Aqua, Color.Bisque, Color.Chocolate, Color.Blue, Color.Goldenrod };
        var color = colors[Random.Shared.Next(0, colors.Count)];
            
        if (currentLobby is not null)
        {
            await connection.InvokeAsync(ServerHandlers.Lobby.ChangeColor, currentLobby.Id, color.ToArgb());
        }
        else
        {
            Console.WriteLine("Current lobby is null");
        }
    }
    else if (message == "healthcheck")
    {
        await connection.InvokeAsync(ServerHandlers.Session.HealthCheck);
    }
    else if (message == "research or colonize")
    {
        var heroId = Guid.NewGuid();
            
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        heroId = userNumber == "1" ? hero1 : hero2;

        Console.WriteLine("Enter planed ID: ");
        var planetId = Console.ReadLine();
        
        var request = new UpdatePlanetStatusRequest
        {
            HeroId = heroId,
            SessionId = sessionId,
            PlanetId = Guid.Parse(planetId)
        };
        await connection.InvokeAsync(ServerHandlers.Session.PostResearchOrColonizePlanet, request);
    }
    else if (message == "next-turn")
    {
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = userNumber == "1" ? hero1 : hero2;
        await connection.InvokeAsync(ServerHandlers.Session.NextTurn, new NextTurnRequest { SessionId = sessionId, HeroId = heroId});
    }
    else if (message == "fort")
    {
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = userNumber == "1" ? hero1 : hero2;
        
        Console.WriteLine("Enter planed ID: ");
        var planetId = Console.ReadLine();
        
        var request = new UpdatePlanetStatusRequest
        {
            HeroId = heroId,
            SessionId = sessionId,
            PlanetId = Guid.Parse(planetId)
        };
        await connection.InvokeAsync(ServerHandlers.Session.BuildFortification, request);
    }
    else if (message == "attack")
    {
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = userNumber == "1" ? hero1 : hero2;
        
        Console.WriteLine("Enter planed ID to attack: ");
        var planetIdToAttack = Console.ReadLine();
        
        Console.WriteLine("Enter planed ID from attack: ");
        var planetIdFrom = Console.ReadLine();
        
        var request = new StartBattleRequest
        {
            HeroId = heroId,
            AttackedPlanetId = Guid.Parse(planetIdToAttack),
            FromPlanetId = Guid.Parse(planetIdFrom),
            CountOfSoldiers = 10
        };
        
        await connection.InvokeAsync(ServerHandlers.Session.StartBattle, request);
    }
    else if (message == "defend")
    {
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = userNumber == "1" ? hero1 : hero2;
        
        Console.WriteLine("Enter planed ID to defend: ");
        var planetIdToAttack = Console.ReadLine();

        var request = new DefendPlanetRequest
        {
            HeroId = heroId,
            AttackedPlanetId = Guid.Parse(planetIdToAttack),
            CountOfSoldiers = 10
        };
        await connection.InvokeAsync(ServerHandlers.Session.DefendPlanet, request);
    }
    else if (message == "get-data")
    {
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = userNumber == "1" ? hero1 : hero2;
        
        await connection.InvokeAsync(ServerHandlers.Session.GetHeroData, heroId);
    }
    else if (message == "session-exit")
    {
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = userNumber == "1" ? hero1 : hero2;
        
        var request = new ExitFromSessionRequest
        {
            HeroId = heroId,
            SessionId = sessionId
        };
        await connection.InvokeAsync(ServerHandlers.Session.ExitFromSession, request);
    }

    return false;
}

Action<string> HandleStringMessageOutput()
{
    return (message) =>
    {
        Console.WriteLine(message);
    };
}

Action<UpdatedPlanetStatusResponse> HandlePlanetActionResponse()
{
    return (planetActionResponse) =>
    {
        Console.WriteLine("Received session");
        string json = JsonSerializer.Serialize(planetActionResponse, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
    };
}