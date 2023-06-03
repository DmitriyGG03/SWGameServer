namespace SharedLibrary.Contracts.Hubs
{
    public static class ClientHandlers
    {
        public const string ErrorHandler = "ErrorHandler";
        public static class Lobby
        {
            public const string HealthHandler = "HealthHandler";
            public const string ConnectToLobbyHandler = "ConnectHanler";
            public const string Error = "ErrorHandler";
            public const string DeleteLobbyHandler = "DeleteLobbyHandler";
            public const string ExitFromLobbyHandler = "ExitFromLobbyHanlder";
            public const string ChangeLobbyDataHandler = "Change lobby data hanlder";
            public const string CreatedSessionHandler = "CreatedSessionHandler";
            public const string ChangeReadyStatus = "ChangeReadyStatus";
            public const string ChangedColor = "ChangedColor";
        }
        public static class Session
        {
            public const string PostResearchOrColonizeErrorHandler = "PostResearchOrColonizeErrorHandler";
            public const string IterationDone = "IterationDone";
            public const string ResearchedPlanet = "Researched";
            public const string Colonizing = "Colonized";
            public const string HealthCheckHandler = "healthcheck";
            public const string ReceiveHeroMap = "ReceiveHeroMap";
            public const string Researching = "StartedResearching";
            public const string ReceiveSession = "ReceiveSession";
        }
    }
}