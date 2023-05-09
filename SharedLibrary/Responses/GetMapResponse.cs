using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    public class GetMapResponse : ResponseBase
    {
        public SessionMap? Map { get; set; }
    }
}
