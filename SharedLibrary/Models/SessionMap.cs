﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    [Table("SessionMaps"), Serializable]
    public class SessionMap
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }
        public List<Planet> Planets { get; set; }
        public List<Edge> Connections { get; set; }
        
        public Session? Session { get; set; }
        public SessionMap()
        {
            Id = Guid.Empty;
            Planets = new List<Planet>();
            Connections = new List<Edge>();
        }
        public SessionMap(List<Planet> planets, List<Edge> connections)
        {
            Id = Guid.NewGuid();
            Planets = planets;
            Connections = connections;
        }
    }
}
