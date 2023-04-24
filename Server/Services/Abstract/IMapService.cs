using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface IMapService
{
    Task<ServiceResult<SessionMap>> GetMapAsync(int heroId, MapGenerationOptions options, CancellationToken cancellationToken);
    Task<ServiceResult<List<HeroMap>>> GetHeroMaps(List<Hero> heroes, SessionMap sessionMap, CancellationToken cancellationToken);

}