using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public interface IHeroService
{
    void DoSomething();
}
public class HeroService : IHeroService
{
    private readonly GameDbContext _dbContext;
    public HeroService(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void DoSomething() => Console.WriteLine("Doing something");
}