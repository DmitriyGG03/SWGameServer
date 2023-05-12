using System;

namespace SharedLibrary.Models
{
    [Serializable]
    public class MapGenerationOptions
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int MinDistanceBetweenPlanets { get; set; }
        public int MinDistanceFromPlanetToEdge { get; set; }
        public int NumberOfPlanets { get; set; }

        public MapGenerationOptions(int width, int height, int minDistanceBetweenPlanets, int minDistanceFromPlanetToEdge, int numberOfPlanets)
        {
            Width = width;
            Height = height;
            MinDistanceBetweenPlanets = minDistanceBetweenPlanets;
            MinDistanceFromPlanetToEdge = minDistanceFromPlanetToEdge;
            NumberOfPlanets = numberOfPlanets;
        }
    }   
}