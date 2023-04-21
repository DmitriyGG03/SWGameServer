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
        public const string SameName = "There is a lobby with the same name";
        public const string ThereIsUsers = "There is users in lobby";
    }
}