using System;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Requests
{
    public class ResearchColonizePlanetRequest
    {
        [Required] public Guid SessionId { get; set; }
        [Required] public Guid PlanetId { get; set; }
        [Required] public Guid HeroId { get; set; }
    }
}