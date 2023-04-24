using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;

var username = Guid.NewGuid().ToString();
const int port = 7148;
const string hubName = "lobby";
string accessToken = string.Empty;
Guid lobbyId = Guid.Parse("6d4806df-4f4e-41f5-8533-adba34cfc770");

Console.WriteLine("Choose the user: ");
var user = Console.ReadLine();

if(user == "0")
{
    accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjA3NTQ0NSwiZXhwIjoxOTk3Njk0NjQ0LCJpYXQiOjE2ODIwNzU0NDV9.ssFAgkNQJvNS9AfCiawsYixPWM7cwL8GquPCD5BTsZE";
}
else
{
    accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjEiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjA3NTQ0NSwiZXhwIjoxOTk3Njk0NjQ0LCJpYXQiOjE2ODIwNzU0NDV9.ssFAgkNQJvNS9AfCiawsYixPWM7cwL8GquPCD5BTsZE";
}
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
    connection.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
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
        else if (message == "change")
        {
            var lobbys = new Lobby();
            lobbys.LobbyName = "New Lobby";
            lobbys.MaxHeroNumbers = 10;
            lobbys.Id = lobbyId;
            await connection.InvokeAsync(ServerHandlers.Lobby.ChangeLobbyData, lobbys);
        }
    }
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    throw;
}

Console.ReadKey();