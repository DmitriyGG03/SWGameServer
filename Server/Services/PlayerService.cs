namespace Server.Services;

public interface IPlayerService
{
    void DoSomething();
}
public class PlayerService : IPlayerService
{
    public void DoSomething() => Console.WriteLine("Doing something");
}

public class MockPlayerService : IPlayerService
{
    public void DoSomething() => Console.WriteLine("Doing something in a mock service");
}