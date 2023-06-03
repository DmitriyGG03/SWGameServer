using System.Drawing;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

var sessionId = Guid.Parse("4eac29ce-a57b-4c58-8925-10facad36480");
string hero1 = "d8b9cc89-6820-4f13-9e17-8ef5d0ed5cd6";
string hero2 = "7c57de2c-218f-40b4-ad08-fef266cccaff";
var planetId = Guid.Parse("7341ac5e-0e9e-4c3a-8b38-7c7622b2f01c");

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
    hubConnection.On<string>(ClientHandlers.Session.PostResearchOrColonizeErrorHandler, HandleStringMessageOutput());
    hubConnection.On<string>(ClientHandlers.Session.HealthCheckHandler, HandleStringMessageOutput());
    
    hubConnection.On<PlanetActionResponse>(ClientHandlers.Session.ResearchingPlanet, HandlePlanetActionResponse());
    hubConnection.On<PlanetActionResponse>(ClientHandlers.Session.ColonizingPlanet, HandlePlanetActionResponse());

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
        var heroId = Guid.NewGuid();
            
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        heroId = Guid.Parse(userNumber == "1" ? hero1 : hero2);
        
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
        Console.WriteLine("Are you 1 or 2 user?");
        var userNumber = Console.ReadLine();
        var heroId = Guid.Empty;
        heroId = Guid.Parse(userNumber == "1" ? hero1 : hero2);
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

Action<PlanetActionResponse> HandlePlanetActionResponse()
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