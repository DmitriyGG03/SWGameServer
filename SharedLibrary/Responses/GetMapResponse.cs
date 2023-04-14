using SharedLibrary.Models;

namespace SharedLibrary.Responses
{
    public class GetMapResponse
    {
        public SessionMap? Map { get; set; }
        public string[]? Info { get; set; }
    }
}
