using System.Drawing;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;

var username = Guid.NewGuid().ToString();
const int port = 7148;
const string hubName = "lobby";
string accessToken = string.Empty;
Guid lobbyId = Guid.Parse("25da84c2-7ded-48a0-9baa-029d694a9b40");

Console.WriteLine("Choose the user: ");
var user = Console.ReadLine();

if(user == "0")
{
    accessToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6ImQxN2YwZDcyLTcyZmYtNDk5MS1hNTg2LWNkMDg5NmIxMjlhYiIsImhlcm8iOiJudWxsIiwibmJmIjo" +
        "xNjgzODA1MTg5LCJleHAiOjE5OTk0MjQzODksImlhdCI6MTY4MzgwNTE4OX0.SvzQ_Qwv655cAvqnOLXvA1EAvr1LHVWyT7tp0MSWaV0";
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
        Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready + "; " + info.Color);
    });
    
    hubConnection.On<LobbyInfo>(ClientHandlers.Lobby.ChangeReadyStatus, (info) =>
    {
        Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready + "; " + info.Color);
    });
    
    hubConnection.On<Guid>(ClientHandlers.Lobby.CreatedSessionHandler, (sessionId) =>
    {
        Console.WriteLine("Session id: " + sessionId);
    });
    
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

        /*
        if (currentLobby is not null)
        {
            await connection.InvokeAsync(ServerHandlers.Lobby.CreateSession, new Lobby {Id = lobbyId });
        }
        else
        {
            Console.WriteLine("Current lobby is null");
        }
         */
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

    return false;
}