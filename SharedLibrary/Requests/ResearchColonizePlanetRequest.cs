using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Requests
{
    public class ResearchColonizePlanetRequest
    {
        [Required]
        public Guid PlanetId { get; set; }
    }
}