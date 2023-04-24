namespace SharedLibrary.Contracts.Hubs
{
    public static class ClientHandlers
    {
        public static class Lobby
        {
            public const string HealthHandler = "HealthHandler";
            public const string ConnectToLobbyHandler = "ConnectHanler";
            public const string Error = "ErrorHandler";
            public const string DeleteLobbyHandler = "DeleteLobbyHandler";
            public const string ExitFromLobbyHandler = "Exit from lobby hanlder";
            public const string ChangeLobbyDataHandler = "Change lobby data hanlder";
        }
    }
}