namespace Server.Repositories;

public interface IPlanetNameRepository
{
    public ICollection<string> PlanetNames { get; }
}