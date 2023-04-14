namespace drawing_test.Models;

public class MapGenerationOptions
{
    public int Width { get; }
    public int Height { get; }
    public int MinDistanceBetweenPlanets { get; }
    public int MinDistanceFromPlanetToEdge { get; }
    public int NumberOfPlanets { get; }

    public MapGenerationOptions(int width, int height, int minDistanceBetweenPlanets, int minDistanceFromPlanetToEdge, int numberOfPlanets)
    {
        Width = width;
        Height = height;
        MinDistanceBetweenPlanets = minDistanceBetweenPlanets;
        MinDistanceFromPlanetToEdge = minDistanceFromPlanetToEdge;
        NumberOfPlanets = numberOfPlanets;
    }
}