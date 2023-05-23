using System;
using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class GetHeroResponse : ResponseBase
    {
        public Hero? Hero { get; set; }
        public HeroMapView? Map { get; set; }
    }
}