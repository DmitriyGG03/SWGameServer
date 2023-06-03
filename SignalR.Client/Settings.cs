using SharedLibrary.Models;

namespace SignalR.Client;

public class Settings
{
    public Guid LobbyId { get; set; }
    public Session? Session { get; set; }
}