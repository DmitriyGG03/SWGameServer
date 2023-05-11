using System;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class CreateHeroResponse : ResponseBase
    {
        public Guid HeroId { get; set; }

        public CreateHeroResponse()
        {
            HeroId = Guid.Empty;
        }
    }
}