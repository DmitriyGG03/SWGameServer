using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    public class CreateHeroResponse : ResponseBase
    {
        public int HeroId { get; set; }

        public CreateHeroResponse()
        {
            HeroId = -1;
        }
    }
}