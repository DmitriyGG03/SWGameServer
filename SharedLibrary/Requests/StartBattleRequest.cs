using System;

namespace SharedLibrary.Requests
{
    public class StartBattleRequest
    {
        public Guid HeroId { get; set; }
        public Guid AttackedPlanetId { get; set; }
        public Guid FromPlanetId { get; set; }
        public int CountOfSoldiers { get; set; }
    }
}