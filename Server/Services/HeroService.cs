namespace Server.Services;

public interface IHeroService
{
    void DoSomething();
}
public class HeroService : IHeroService
{
    public void DoSomething() => Console.WriteLine("Doing something");
}