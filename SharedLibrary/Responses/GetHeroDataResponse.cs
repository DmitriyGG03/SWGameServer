using System.Collections.Generic;
using SharedLibrary.Models;

namespace SharedLibrary.Responses
{
    public class GetHeroDataResponse
    {
        public Session? Session { get; set; }
        public Hero? Hero { get; set; }
        public HeroMapView? HeroMapView { get; set; }
        public List<Battle>? Battles { get; set; }
    }
}