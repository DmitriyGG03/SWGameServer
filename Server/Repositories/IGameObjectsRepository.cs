using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server.Repositories;

public interface IGameObjectsRepository
{
    Task<List<Planet>> GetNeighborPlanetsAsync(Guid planetId, CancellationToken cancellationToken);

    Task<HeroPlanetRelation?> GetRelationByHeroAndPlanetIdsAsync(Guid heroId, Guid planetId, CancellationToken cancellationToken);
    
    Task<HeroPlanetRelation?> GetUnknownRelationByHeroAndPlanetIdsAsync(Guid heroId, Guid planetId, CancellationToken cancellationToken);

    void UpdateHeroPlanetRelations(List<HeroPlanetRelation> relations);
}

public class GameObjectRepository : IGameObjectsRepository
{
    private readonly GameDbContext _context;

    public GameObjectRepository(GameDbContext context)
    {
        _context = context;
    }

    public async Task<List<Planet>> GetNeighborPlanetsAsync(Guid planetId, CancellationToken cancellationToken)
    {
        var neighborPlanetsIds = await _context.Connections
            .Where(x => x.FromPlanetId == planetId) 
            .Select(x => x.ToPlanetId)
            .ToListAsync(cancellationToken);
            
        var neighborPlanets = new List<Planet>();
        foreach (var id in neighborPlanetsIds)
        {
            var planet = await _context.Planets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (planet is null)
                throw new InvalidOperationException();
            neighborPlanets.Add(planet);
        }

        return neighborPlanets;
    }

    public async Task<HeroPlanetRelation?> GetRelationByHeroAndPlanetIdsAsync(Guid heroId, Guid planetId, CancellationToken cancellationToken)
    {
        HeroPlanetRelation? relation = await _context.HeroPlanetRelations
            .Include(x => x.Hero)
            .Include(x => x.Planet)
            .FirstOrDefaultAsync(x =>
            x.HeroId == heroId &&
            x.PlanetId == planetId &&
            x.Status >= PlanetStatus.Known, cancellationToken);

        return relation;
    }

    public async Task<HeroPlanetRelation?> GetUnknownRelationByHeroAndPlanetIdsAsync(Guid heroId, Guid planetId, CancellationToken cancellationToken)
    {
        HeroPlanetRelation? relation = await _context.HeroPlanetRelations.FirstOrDefaultAsync(x =>
                x.HeroId == heroId &&
                x.PlanetId == planetId &&
                x.Status == PlanetStatus.Unknown, cancellationToken);

        return relation;
    }

    public void UpdateHeroPlanetRelations(List<HeroPlanetRelation> relations)
    {
        _context.HeroPlanetRelations.UpdateRange(relations);
    }
}