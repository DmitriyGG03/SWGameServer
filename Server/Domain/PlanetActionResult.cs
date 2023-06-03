using SharedLibrary.Models;

namespace Server.Domain;

public record PlanetActionResult(PlanetStatus RelationStatus, int IterationsToTheNextStatus);