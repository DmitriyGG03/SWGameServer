using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;

var username = Guid.NewGuid().ToString();
const int port = 7148;
const string hubName = "lobby";
string accessToken = string.Empty;
Guid lobbyId = Guid.Parse("AAA2441D-04B4-4357-BA04-218513A1213C");

Console.WriteLine("Choose the user: ");
var user = Console.ReadLine();

if(user == "0")
{
    accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjUiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjQyOTU4NywiZXhwIjoxOTk4MDQ4Nzg3LCJpYXQiOjE2ODI0Mjk1ODd9.LfSP4PpvU8uGIsxV5BqnZRRaByZBvGwFt6rhoRXTvFQ";
}
else
{
    accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjQyOTUzMSwiZXhwIjoxOTk4MDQ4NzMxLCJpYXQiOjE2ODI0Mjk1MzF9.nNGq-E9N_4JaRD6ZdTA4HTlsudWGWt4zVrgodR8z1ns";
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
    
    connection.On<string>(ClientHandlers.Lobby.Error, (errorMessage) =>
    {
        var message = $"Server error: {errorMessage}";
        Console.WriteLine(message);
    });
    connection.On<string>(ClientHandlers.Lobby.DeleteLobbyHandler, (serverMessage) =>
    {
        var message = $"Server message: {serverMessage}";
        Console.WriteLine(message);
    });
    connection.On<Lobby>(ClientHandlers.Lobby.ConnectToLobbyHandler, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }
        currentLobby = lobby;
    });
    connection.On<Lobby>(ClientHandlers.Lobby.ChangeReadyStatus, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }
        currentLobby = lobby;
    });
    connection.On<Lobby>(ClientHandlers.Lobby.ExitFromLobbyHandler, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }
        currentLobby = lobby;
    });
    connection.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }

        currentLobby = lobby;
    });
    connection.On<Hero>(ClientHandlers.Lobby.CreatedSessionHandler, (hero) =>
    {
        Console.WriteLine("Hero name: " + hero.Name);
        Console.WriteLine("Planets count: " + hero.HeroMap.Planets.Count);
        foreach (var item in hero.HeroMap.Planets)
        {
            Console.WriteLine("X=" + item.Position.X + "; Y=" + item.Position.Y);
        }

        Console.WriteLine("Home planet cords: " + "X=" + hero.HeroMap.HomePlanet.Position.X + "; Y=" + hero.HeroMap.HomePlanet.Position.Y);
    });
    
    await connection.StartAsync();
    
    while (true)
    {
        Console.WriteLine("Type message to execute operation: ");
        string message = Console.ReadLine();
        if(message == "exit")
            break;

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
            if (currentLobby is not null)
            {
                await connection.InvokeAsync(ServerHandlers.Lobby.CreateSession, currentLobby);
            }
            else
            {
                Console.WriteLine("Current lobby is null");
            }
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
    }
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    throw;
}

Console.ReadKey();