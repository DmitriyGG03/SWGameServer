using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SharedLibrary.Models
{
    [Table("Edges"), Serializable]
    public class Edge
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }
        [NotMapped, IgnoreDataMember]
        public Planet? From { get; set; }
        public Guid FromPlanetId { get; set; }
        [NotMapped, IgnoreDataMember]
        public Planet? To { get; set; }
        public Guid ToPlanetId { get; set; }
        /* ctor for deserialization */
        public Edge()
        { }

        public Edge(Planet from, Planet to)
        {
            Id = Guid.NewGuid();
            From = from;
            FromPlanetId = from.Id;
            To = to;
            ToPlanetId = to.Id;
        }
    }
}
