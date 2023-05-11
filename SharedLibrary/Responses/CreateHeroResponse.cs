using System;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class CreateHeroResponse : ResponseBase
    {
        public int HeroId { get; set; }

        public CreateHeroResponse()
        {
            HeroId = -1;
        }
    }
}