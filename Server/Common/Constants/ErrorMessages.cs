namespace Server.Common.Constants;

public static class ErrorMessages
{
    public static class User
    {
        public const string NotFound = "There is no user with given id";
        public const string HasNoAccess = "User has no access to hero with given id";   
    }
    public static class Hero
    {
        public const string NotFound = "There is no hero with given id";
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
    public static class Session
    {
        public const string NotFound = "There is no session with given id";
        public const string NotEnoughResearchShips = "Given hero does not have enough research ships";
        public const string NotEnoughColonizationShips = "Given hero does not have enough colonization ships";
        public const string NotHeroTurn = "We are sorry, but now it isn't your turn";
        public const string NotEnoughSoldiers = "You do not have enough soldiers to attack";
        public const string CanNotStartResearchingOrColonization = "We can't start researching or colonizetion of given planet";
    }
    
    public static class Relation
    {
        public const string NotFound = "There is no relation with given parameters";
    }
    
    public static class Battle
    {
        public const string NotFound = "There is no battle with given parameters";
    }

    public static class Planet
    {
        public const string NotFound = "There is no planet with given id";
    }
}