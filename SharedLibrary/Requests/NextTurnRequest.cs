using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Requests
{
    public class NextTurnRequest
    {
        [Required]
        public Guid SessionId { get; set; }
    }
}