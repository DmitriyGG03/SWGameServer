namespace Server.Services.Abstract;

public interface IPlanetNameGenerator
{
    public string GenerateRandomPlanetName();
    public string GeneratePlanetNameBasedOnUniqueIndex(int index);
}