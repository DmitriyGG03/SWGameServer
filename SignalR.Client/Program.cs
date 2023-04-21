using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;

var username = Guid.NewGuid().ToString();
const int port = 44355;
const string hubName = "lobby";
const string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjMiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjA3MzcxNywiZXhwIjoxOTk3NjkyOTE3LCJpYXQiOjE2ODIwNzM3MTd9.dFeDG-ypFgOwEV3httO4ua5WmnW9f7XcrysUu2AR13g";
const string accessTokenSignalRUser =
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjA3NjU1NCwiZXhwIjoxOTk3Njk1NzU0LCJpYXQiOjE2ODIwNzY1NTR9.Qm2_mDnptR5DmkLYgm2Ha6zWL9_cfFh8JGKzs812hbc";
Guid lobbyId = Guid.Parse("aaa2441d-04b4-4357-ba04-218513a1213c");

try
{
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
    });
    connection.On<Lobby>(ClientHandlers.Lobby.ExitFromLobbyHandler, (lobby) =>
    {
        Console.WriteLine(lobby.LobbyName + ": \n");
        foreach (var info in lobby.LobbyInfos)
        {
            Console.WriteLine(info.UserId + ": " + info.User?.Username + "; " + info.Ready);
        }
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
    }
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    throw;
}

Console.ReadKey();