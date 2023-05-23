using System.Net.NetworkInformation;

namespace SharedLibrary.Contracts.Hubs
{
    public static class ServerHandlers
    {
        public static class Lobby
        {
            public const string HealthCheck = "HealthCheck";
            public const string ConnectToLobby = "ConnectToLobby";
            public const string ExitFromLobby = "ExitFromLobby";
            public const string ChangeLobbyData = "ChangeLobbyData";
            public const string CreateSession = "CreateSession";
            public const string ChangeReadyStatus = "ChangeReadyStatus";
            public const string ChangeColor = "ChangeColor";
        }
        
        public static class Session
        {
            public const string PostResearchOrColonizePlanet = "PostResearchOrColonizePlanet";
            public const string HealthCheck = "HealthCheck";
        }
    }
}