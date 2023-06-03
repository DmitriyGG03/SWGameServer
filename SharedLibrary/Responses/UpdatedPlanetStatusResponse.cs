using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace SharedLibrary.Responses
{
    public class UpdatedPlanetStatusResponse
    {
        public PlanetStatus RelationStatus { get; set; }
        public int IterationsToTheNextStatus { get; set; }
    }
}