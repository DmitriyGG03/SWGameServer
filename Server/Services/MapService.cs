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
}