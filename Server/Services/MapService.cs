using Microsoft.EntityFrameworkCore;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public class MapService : IMapService
{
    private readonly GameDbContext _context;
    private readonly IMapGenerator _mapGenerator;
    public MapService(GameDbContext context, IMapGenerator mapGenerator)
    {
        _context = context;
        _mapGenerator = mapGenerator;
    }
    
    public async Task<ServiceResult<SessionMap>> GetMapAsync(int heroId, MapGenerationOptions options, CancellationToken cancellationToken)
    {
        var hero = await _context.Heroes.FirstOrDefaultAsync(x => x.HeroId == heroId, cancellationToken);
        if (hero == null)
        {
            return new ServiceResult<SessionMap>("There is no session with given id");
        }

        var exists = await _context.SessionMaps
            .Include(x => x.Planets)
            .ThenInclude(x => x.Position)
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.HeroId == heroId, cancellationToken);

        if (exists != null)
            return new ServiceResult<SessionMap>(exists);

        var map = _mapGenerator.GenerateMap(options);
        map.HeroId = heroId;
        await _context.SessionMaps.AddAsync(map, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ServiceResult<SessionMap>(map);
    }
    public async Task<ServiceResult<List<HeroMap>>> GetHeroMaps(List<Hero> heroes, SessionMap sessionMap, CancellationToken cancellationToken)
    {
        List<Planet> listPlanet = new List<Planet>();
        var heroMapList = new List<HeroMap>();

        var rnd = new Random();
        for (int i = 0; i < heroes.Count; i++)
        {
            Planet planet = new Planet();
            var heroMap = new HeroMap();
            do
            {
                var rndIndex = rnd.Next(sessionMap.Planets.Count);
                planet = sessionMap.Planets[rndIndex];
            } while (listPlanet.Contains(planet));
            listPlanet.Add(planet);
            heroMap.HeroId = heroes[i].HeroId;
            // heroMap.Id = i;
            // heroMap.Hero = heroes[i];
            heroMap.Connections = sessionMap.Connections;
            heroMap.Planets = sessionMap.Planets;
            heroMap.HomePlanet = planet;
            heroMap.HomePlanetId = Guid.NewGuid();
            heroMapList.Add(heroMap);
        }
        return new ServiceResult<List<HeroMap>>(heroMapList);
    }
}