using SharedLibrary.Models.Enums;

namespace SharedLibrary.Responses
{
    public class UpdatedFortificationResponse
    {
        public Fortification Fortification { get; set; }
        public int IterationsToTheNextStatus { get; set; }
    }
}