using System;
using SharedLibrary.Models;
using SharedLibrary.Responses.Abstract;

namespace SharedLibrary.Responses
{
    [Serializable]
    public class UpdateHeroResponse : ResponseBase
    {
        public Hero? Hero { get; set; }
    }
}