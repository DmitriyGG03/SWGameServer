using System;

namespace SharedLibrary.Requests
{
    public class ExitFromSessionRequest
    {
        public Guid HeroId { get; set; }
        public Guid SessionId { get; set; }
    }
}