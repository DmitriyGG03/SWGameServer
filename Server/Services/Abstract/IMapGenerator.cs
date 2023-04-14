using SharedLibrary.Models;

namespace Server.Services.Abstract;

public interface IMapGenerator
{
    Map GenerateMap(MapGenerationOptions options);
}