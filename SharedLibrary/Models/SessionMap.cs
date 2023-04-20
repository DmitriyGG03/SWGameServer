using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    public class SessionMap
    {
        public Guid Id { get; set; }
        public List<Planet> Planets { get; set; }
        public List<Edge> Connections { get; set; }
        
        [ForeignKey(nameof(Hero))]
        public int HeroId { get; set; }
        public Hero? Hero { get; set; }

        public Session? Session { get; set; }
        
        /* constructor for deserialization */
        public SessionMap() { }
        public SessionMap(List<Planet> planets, List<Edge> connections)
        {
            Id = Guid.NewGuid();
            Planets = planets;
            Connections = connections;
        }
    }
}
