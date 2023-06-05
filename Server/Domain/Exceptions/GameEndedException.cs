using SharedLibrary.Models;

namespace Server.Domain.Exceptions;

public class GameEndedException : Exception
{
    public GameEndedException(Hero winner) : base("Game has been ended")
    {
        Winner = winner;
    }

    public Hero? Winner { get; set; }
}