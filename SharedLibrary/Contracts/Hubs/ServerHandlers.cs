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
        }
    }
}