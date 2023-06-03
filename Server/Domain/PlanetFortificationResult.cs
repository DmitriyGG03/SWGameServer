using SharedLibrary.Models.Enums;

namespace Server.Domain;

public record PlanetFortificationResult(Fortification Fortification, int IterationToNextStatus);