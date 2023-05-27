using System.Drawing;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;

/* Constants:
    Access token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6Ijc3ZTlhM2JhLWIxZTItNDY1Yi1iMjU4LTczMzQ1MjM0ZTE2NSIsImhlcm8iOiJudWxsIiwibmJmIjoxNjgzOTc0NDg4LCJleHAiOjE5OTk1OTM2ODgsImlhdCI6MTY4Mzk3NDQ4OH0.QiZ-LQIi9peuzLmCxAWgsCNoRRf0h9g2_lLqpmUoAIo
    Lobby id: de7c3558-1a03-40a3-92a3-369a520977ed
 */

const int port = 7148;
Console.WriteLine("Enter hub name");
string hubName = Console.ReadLine();

Console.WriteLine("Enter access token: ");
var accessToken = Console.ReadLine();

var lobbyId = Guid.Empty;
if (hubName == "lobby")
{
    Console.WriteLine("Enter lobby id, that you want to use: ");
    var lobbyIdRaw = Console.ReadLine();
    
    if (Guid.TryParse(lobbyIdRaw, out lobbyId) == false)
    {
        throw new ArgumentException("Given lobby id has wrong format");
    }
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

    hubConnection.On<HeroMapView>(ClientHandlers.Session.ResearchedPlanet, (heroMap) =>
    {
        string json = JsonSerializer.Serialize(heroMap, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        Console.WriteLine(json);
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
    
    hubConnection.On<string>(ClientHandlers.ErrorHandler, HandleStringMessageOutput());
    hubConnection.On<string>(ClientHandlers.Session.StartedResearching, HandleStringMessageOutput());
    hubConnection.On<string>(ClientHandlers.Session.StartedColonizingPlanet, HandleStringMessageOutput());
    hubConnection.On<string>(ClientHandlers.Session.IterationDone, HandleStringMessageOutput());
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
        await connection.InvokeAsync(ServerHandlers.Lobby.ConnectToLobby, lobbyId);
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
        var planetId = Guid.Parse("0b4778b1-780f-4bc9-82e6-a860d5516a6c");
        var heroId = Guid.Parse("ebe9eac1-104a-4f09-9d17-31ee0ee38858");
        var sessionId = Guid.Parse("5d98b407-7092-4813-b2c5-fe7c2182ed85");
        
        var request = new ResearchColonizePlanetRequest
        {
            HeroId = heroId,
            SessionId = sessionId,
            PlanetId = planetId
        };
        await connection.InvokeAsync(ServerHandlers.Session.PostResearchOrColonizePlanet, request);
    }
    else if (message == "next-turn")
    {
        var sessionId = Guid.Parse("f5b94058-3fb8-4147-903d-01cddf03057e");
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = Guid.Parse(userNumber == "1" ? "5b93fabf-0c50-4553-9aca-c93271c121e3" : "0b770df7-569b-463f-ae5f-e8712f328885");
        await connection.InvokeAsync(ServerHandlers.Session.NextTurn, new NextTurnRequest { SessionId = sessionId, HeroId = heroId});
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