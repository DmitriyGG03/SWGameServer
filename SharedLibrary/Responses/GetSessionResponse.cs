using System;
using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class GetSessionResponse : ResponseBase
    {
        public Session? Session { get; set; }
    }
}