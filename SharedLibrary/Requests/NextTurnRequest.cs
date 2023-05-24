using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Requests
{
    public class NextTurnRequest
    {
        [Required]
        public Guid HeroId { get; set; }
        [Required]
        public Guid SessionId { get; set; }
    }
}