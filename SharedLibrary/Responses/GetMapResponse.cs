using System;
using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class GetMapResponse : ResponseBase
    {
        public SessionMap? Map { get; set; }
    }
}
