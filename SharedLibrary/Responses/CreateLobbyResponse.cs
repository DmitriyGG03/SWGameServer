using System;
using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class CreateLobbyResponse : ResponseBase
    {
        public Lobby? Lobby { get; set; }
    }
}