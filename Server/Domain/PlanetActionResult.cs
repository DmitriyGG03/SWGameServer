using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain;

public record PlanetActionResult(PlanetStatus RelationStatus, Fortification FortificationLevel, int IterationsToTheNextStatus);