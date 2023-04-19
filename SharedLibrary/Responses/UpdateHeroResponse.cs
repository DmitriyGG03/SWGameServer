using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    public class UpdateHeroResponse : ResponseBase
    {
        public Hero? Hero { get; set; }
    }
}