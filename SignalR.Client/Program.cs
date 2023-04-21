using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;

var username = Guid.NewGuid().ToString();
const int port = 44355;
const string hubName = "lobbyHub";

try
{
    var connection = new HubConnectionBuilder()
        .WithUrl($"https://localhost:{port}/{hubName}")
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