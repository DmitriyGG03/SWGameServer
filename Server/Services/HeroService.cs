using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public interface IHeroService
{
    void DoSomething();
    Task<ServiceResult<SessionMap>> GetMapAsync(int heroId, MapGenerationOptions options, CancellationToken cancellationToken);
}
public class HeroService : IHeroService
{
    private readonly GameDbContext _dbContext;
    private readonly IMapGenerator _mapGenerator;
    public HeroService(GameDbContext dbContext, IMapGenerator mapGenerator)
    {
        _dbContext = dbContext;
        _mapGenerator = mapGenerator;
    }

    public void DoSomething() => Console.WriteLine("Doing something");
    public async Task<ServiceResult<SessionMap>> GetMapAsync(int heroId, MapGenerationOptions options, CancellationToken cancellationToken)
    {
        var hero = await _dbContext.Heroes.FirstOrDefaultAsync(x => x.HeroId == heroId);
        if (hero == null)
        {
            return new ServiceResult<SessionMap>("There is no hero with given id");
        }

        var exists = await _dbContext.SessionMaps
            .Include(x => x.Planets)
             .ThenInclude(x => x.Position)
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.HeroId == heroId, cancellationToken);

        if (exists != null)
            return new ServiceResult<SessionMap>(exists);

        var map = _mapGenerator.GenerateMap(options);
        map.HeroId = heroId;
        await _dbContext.SessionMaps.AddAsync(map);
        await _dbContext.SaveChangesAsync();
        return new ServiceResult<SessionMap>(map);
    }
}