using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server.Services;

public interface IHeroMapService
{
    Task<HeroMapView?> GetHeroMapAsync(Guid heroId, CancellationToken cancellationToken);
}

public class HeroMapService : IHeroMapService
{
    private readonly GameDbContext _context;

    public HeroMapService(GameDbContext context)
    {
        _context = context;
    }

    public async Task<HeroMapView?> GetHeroMapAsync(Guid heroId, CancellationToken cancellationToken)
    {
        var heroPlanets = await _context.HeroPlanetRelations
            .Include(x => x.Planet)
            .Where(x => x.HeroId == heroId && x.Status >= PlanetStatus.Known)
            .ToListAsync(cancellationToken);

        if (heroPlanets.Any() == false)
            return null;

        var planets = heroPlanets.Select(x =>
        {
            if (x.Planet.OwnerId is not null && x.Planet.OwnerId.Value != heroId)
            {
                x.Planet.IsEnemy = true;
            }
            else
            {
                x.Planet.IsEnemy = false;
            }

            x.Planet.FortificationLevel = x.FortificationLevel;
            x.Planet.IterationsLeftToNextStatus = x.IterationsLeftToTheNextStatus;
            x.Planet.Status = x.Status;
            return x.Planet;
        }).ToList();

        var rootPlanets = planets.Where(x => x.Status >= PlanetStatus.Researched).ToList();
        var connections = await GetConnectionsAsync(rootPlanets);

        var heroMap = new HeroMapView
        {
            HeroId = heroId,
            Planets = planets,
            Connections = connections
        };
        return heroMap;
    }
    
    private async Task<List<Edge>> GetConnectionsAsync(List<Planet> planets)
    {
        var connections = new List<Edge>();
        foreach (var planet in planets)
        {
            var subResult = await _context.Connections
                .Where(x => x.FromPlanetId == planet.Id)
                .ToListAsync();
                
            connections.AddRange(subResult);
        }

        return connections;
    }
}