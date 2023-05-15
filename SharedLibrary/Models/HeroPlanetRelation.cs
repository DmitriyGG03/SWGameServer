using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    // server type -> it will not be transferred to a client (many to many relation)
    [Table("HeroPlanets")]
    public class HeroPlanetRelation
    {
        [ForeignKey(nameof(Hero))]
        public Guid HeroId { get; set; }
        public Hero? Hero { get; set; }
        [ForeignKey(nameof(Planet))]
        public Guid PlanetId { get; set; }
        public Planet? Planet { get; set; }
        
        // connection status
        public int Status { get; set; }
        // number of iterations to colonize or research planet
        [Column("IterationsLeft")]
        public int IterationsLeftToTheNextStatus { get; set; }
    }
}