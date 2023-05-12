using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface IMapService
{
    Task<ServiceResult<SessionMap>> GetMapAsync(Guid sessionId, MapGenerationOptions options, CancellationToken cancellationToken);
    Task<ServiceResult<List<HeroMapView>>> GetHeroMaps(List<Hero> heroes, SessionMap sessionMap, CancellationToken cancellationToken);

}