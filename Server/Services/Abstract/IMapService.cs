using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface IMapService
{
    Task<ServiceResult<SessionMap>> GetMapAsync(int sessionId, MapGenerationOptions options, CancellationToken cancellationToken);
}