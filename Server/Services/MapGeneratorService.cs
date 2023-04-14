using System.Drawing;
using Server.Domain.Math;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public class MapGeneratorService : IMapGenerator
{
    public Map GenerateMap(MapGenerationOptions options)
    {
        if (options == null)
            throw new ArgumentNullException($"{nameof(options)} can not be null");

        var planets = GeneratePlanets(options);
        var connections = GenerateConnectionsBetweenPlanets(planets, options);

        return new Map(planets, connections);
    }

    private List<Planet> GeneratePlanets(MapGenerationOptions options)
    {
        var planets = new List<Planet>();

        for (int i = 0; i < options.NumberOfPlanets; i++)
        {
            var position = Geometry.GenerateRandomPoint(options.Width, options.Height);
            // Calculate distance
            while (planets.Any(
                       p => Geometry.CalculateDistance(p.Position, position) < options.MinDistanceBetweenPlanets))
            {
                position = Geometry.GenerateRandomPoint(options.Width, options.Height);
            }

            var planet = new Planet(position);
            planets.Add(planet);
        }

        return planets;
    }

    private List<Edge> GenerateConnectionsBetweenPlanets(List<Planet> planets, MapGenerationOptions options)
    {
        // Створюємо список з'єднань між планетами
        var connections = new List<Edge>();

        // З'єднуємо кожну планету із найближчим сусідом
        foreach (Planet planet in planets)
        {
            List<Planet> neighbors = planets.OrderBy(p => Geometry.CalculateDistance(p.Position, planet.Position))
                .Where(p => p != planet)
                .Take(3)
                .ToList();

            foreach (Planet neighbor in neighbors)
            {
                var connection = new Edge(planet, neighbor);
                // Перевірте, чи перетинається нове з'єднання з існуючими
                var overlaps = IsConnectionOverlapsOthers(connection, connections);

                if (!overlaps)
                {
                    // Перевірка, що з'єднання не проходить занадто близько до інших планет
                    bool tooClose = IsConnectionTooCloseToSomePlanet(connection, planet, neighbor, planets,
                        options.MinDistanceFromPlanetToEdge);
                    if (!tooClose)
                    {
                        connections.Add(connection);
                        /* - Useless code?
                         planet.Connections.Add(neighbor);
                         neighbor.Connections.Add(planet);
                        */
                    }
                }
            }
        }

        return connections;
    }

    private bool IsConnectionTooCloseToSomePlanet(Edge connectionToCheck, Planet currentPlanet, Planet neighbor,
        List<Planet> planets, int minDistanceFromPlanetToEdge)
    {
        bool tooClose = false;
        foreach (Planet otherPlanet in planets.Where(p => p != currentPlanet && p != neighbor))
        {
            if (Geometry.CalculateDistanceToSegment(connectionToCheck.From.Position, connectionToCheck.To.Position,
                    otherPlanet.Position) < minDistanceFromPlanetToEdge)
            {
                tooClose = true;
                break;
            }
        }

        return tooClose;
    }

    private bool IsConnectionOverlapsOthers(Edge connectionToCheck, List<Edge> connections)
    {
        bool overlaps = false;
        foreach (var existingConnection in connections)
        {
            if (IsConnectionOverlapsOther(connectionToCheck, existingConnection))
            {
                // Якщо з'єднання має спільний початок або кінець, це не перетин
                continue;
            }

            if (Geometry.Intersects(connectionToCheck.From.Position, connectionToCheck.To.Position,
                    existingConnection.From.Position, existingConnection.To.Position))
            {
                overlaps = true;
                break;
            }
        }

        return overlaps;
    }

    public bool IsConnectionOverlapsOther(Edge toCheck, Edge other)
    {
        if (other.From == toCheck.From || other.From == toCheck.To ||
            other.To == toCheck.From || other.To == toCheck.To)
            return true;

        return false;
    }
}