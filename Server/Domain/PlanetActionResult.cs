using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain;

public record PlanetActionResult(PlanetStatus RelationStatus, Fortification FortificationLevel, Guid PlanetId, 
    byte AvailableResearchShips, byte AvailableColonizationShips, int Resources, int IterationsToTheNextStatus);