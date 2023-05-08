namespace Server.Common.Constants;

public static class ErrorMessages
{
    public static class User
    {
        public const string NotFound = "There is no user with given id";
        public const string HasNoAccess = "User has no access to hero with given id";   
    }
    public static class Lobby
    {
        public const string NotFound = "There is no lobby with given id";
        public const string ThereIsUsers = "There is users in lobby";
        public const string UserAlreadyInLobby = "User already in lobby";
        public const string UserIsNotInLobby = "Lobby does not contain user with given id";
        public const string UsersNotReady = "Not all lobby members are ready";
        public const string NoLobbies = "No active lobbies found. You can start a new game";
    }
}