using Server.Repositories;
using Server.Services.Abstract;

namespace Server.Services;

public class PlanetNameGenerator : IPlanetNameGenerator
{
    private readonly IPlanetNameRepository _planetNameRepository;
    public PlanetNameGenerator(IPlanetNameRepository planetNameRepository)
    {
        _planetNameRepository = planetNameRepository;
    }

    public string GenerateRandomPlanetName()
    {
        var planetNames = _planetNameRepository.PlanetNames.ToList();
        var planetName = planetNames[Random.Shared.Next(0, planetNames.Count)];
        return planetName + "-" + GeneratePostfix();
    }
    public string GeneratePlanetNameBasedOnUniqueIndex(int index)
    {
        var planetNames = _planetNameRepository.PlanetNames.ToList();
        if (index < 0 || index >= planetNames.Count)
        {
            throw new ArgumentException($"Index can not be less then 0 or bigger then: {planetNames.Count}");
        }
        
        var planetName = planetNames[index];
        return planetName + "-" + GeneratePostfix();
    }

    private string GeneratePostfix()
    {
        var guid = Guid.NewGuid();
        return guid.ToString().Substring(0, 8);
    }
}