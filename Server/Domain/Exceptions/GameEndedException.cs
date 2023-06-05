using SharedLibrary.Models;

namespace Server.Domain.Exceptions;

public class GameEndedException : Exception
{
    public GameEndedException() : base()
    { }

    public GameEndedException(string message) : base(message)
    { }

    public GameEndedException(string message, Exception exception) : base(message, exception)
    { }

    public GameEndedException(Hero winner) : base()
    {
        Winner = winner;
    }

    public Hero? Winner { get; set; }
}