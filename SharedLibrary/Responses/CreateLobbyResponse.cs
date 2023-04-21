using System;
using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    public class CreateLobbyResponse : ResponseBase
    {
        public Guid LobbyId { get; set; }
    }
}