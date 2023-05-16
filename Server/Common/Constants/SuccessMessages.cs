namespace Server.Common.Constants;

public static class SuccessMessages
{
    public static class Hero
    {
        public const string Created = "Hero has been successfully created";
        public const string Updated = "Hero has been successfully updated";
        public const string Found = "Hero has been successfully found";
    }
    public static class Lobby
    {
        public const string Created = "Lobby has been successfully created";
        public const string Deleted = "Lobby has been successfully deleted";
        public const string Exited = "You have successfully exited the lobby";
        public const string Found = "Lobbies has been successfully found";
    }
    
    public static class Session
    {
        public const string Found = "Session has been successfully found";
        public const string Researched = "Successfully researched planet!";
        public const string Colonized = "Successfully colonized planet!";
        public const string IterationDone = "Successfully done iteration. Left: ";
    }
}