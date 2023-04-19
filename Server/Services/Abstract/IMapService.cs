using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface IMapService
{
    Task<ServiceResult<SessionMap>> GetMapAsync(int heroId, MapGenerationOptions options, CancellationToken cancellationToken);
}