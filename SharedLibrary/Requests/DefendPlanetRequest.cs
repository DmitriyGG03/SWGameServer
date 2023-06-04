using System;

namespace SharedLibrary.Requests
{
    public class DefendPlanetRequest
    {
        public Guid HeroId { get; set; }
        public Guid AttackedPlanetId { get; set; }
        public int CountOfSoldiers { get; set; }
    }
}