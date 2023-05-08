using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    public class GetHeroResponse : ResponseBase
    {
        public Hero? Hero { get; set; }
    }
}