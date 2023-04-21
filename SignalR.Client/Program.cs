using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;

var username = Guid.NewGuid().ToString();
const int port = 44355;
const string hubName = "lobby";
const string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjMiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjA3MzcxNywiZXhwIjoxOTk3NjkyOTE3LCJpYXQiOjE2ODIwNzM3MTd9.dFeDG-ypFgOwEV3httO4ua5WmnW9f7XcrysUu2AR13g";

try
{
    var connection = new HubConnectionBuilder()
        .WithUrl($"https://localhost:{port}/hubs/{hubName}", options =>
        { 
            options.AccessTokenProvider = () => Task.FromResult(accessToken);
        })
        .WithAutomaticReconnect()
        .Build();
    
    connection.On<string>(ClientHandlers.Lobby.HealthHandler, (status) =>
    {
        var newMessage = $"Server status: {status}";
        Console.WriteLine(newMessage);
    });
    await connection.StartAsync();

    while (true)
    {
        Console.WriteLine("Wait for the message: ");
        string message = Console.ReadLine();
        if(message == "exit")
            break;
        
        await connection.InvokeAsync(ServerHandlers.Lobby.HealthCheck);
    }
}
catch (Exception exception)
{
    Console.WriteLine(exception);
    throw;
}

Console.ReadKey();