using SharedLibrary.Models;

namespace SharedLibrary.Responses
{
    public class PlanetActionResponse
    {
        public PlanetStatus RelationStatus { get; set; }
        public int IterationsToTheNextStatus { get; set; }
    }
}