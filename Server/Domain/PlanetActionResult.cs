using SharedLibrary.Models;

namespace Server.Domain;

public class PlanetActionResult
{
    public PlanetStatus RelationStatus { get; set; }
    public int NewCountOfIterations { get; set; }

    public PlanetActionResult()
    {
        RelationStatus = PlanetStatus.Unknown;
        NewCountOfIterations = 0;
    }

    public PlanetActionResult(PlanetStatus relationStatus, int newCountOfIterations)
    {
        RelationStatus = relationStatus;
        NewCountOfIterations = newCountOfIterations;
    }
}
