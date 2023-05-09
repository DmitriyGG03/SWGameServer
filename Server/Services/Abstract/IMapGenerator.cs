using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface IMapGenerator
{
    SessionMap GenerateMap(MapGenerationOptions options);
}