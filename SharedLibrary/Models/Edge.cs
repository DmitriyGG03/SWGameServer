using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SharedLibrary.Models
{
    public class Edge
    {
        public Guid Id { get; set; }
        [NotMapped, JsonIgnore]
        public Planet From { get; set; }
        public Guid FromPlanetId { get; set; }
        [NotMapped, JsonIgnore]
        public Planet To { get; set; }
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
