using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    public class GetLobbyResponse : ResponseBase
    {
        public Lobby? Lobby { get; set; }
    }
}