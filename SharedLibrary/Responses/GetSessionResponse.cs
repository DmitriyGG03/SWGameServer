using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    public class GetSessionResponse : ResponseBase
    {
        public Session? Session { get; set; }
    }
}