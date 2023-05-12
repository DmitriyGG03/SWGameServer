using System;
using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class GetLobbyResponse : ResponseBase
    {
        public Lobby? Lobby { get; set; }
    }
}