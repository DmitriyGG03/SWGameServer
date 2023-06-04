using System;
using SharedLibrary.Models.Enums;

namespace SharedLibrary.Responses
{
    public class UpdatedPlanetStatusResponse
    {
        public PlanetStatus RelationStatus { get; set; }
        public int IterationsToTheNextStatus { get; set; }
        public Guid PlanetId { get; set; }
        public byte AvailableResearchShips { get; set; }
        public byte AvailableColonizationShips { get; set; }
    }
}