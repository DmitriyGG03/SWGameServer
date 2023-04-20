using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
	public class HeroMap
	{
		public int Id { get; set; }
		public int HeroId { get; set; }
		public Hero? Hero { get; set; }
		
		[ForeignKey(nameof(HomePlanet))]
		public Guid HomePlanetId { get; set; }
		public Planet HomePlanet { get; set; }
		
		public List<Planet> Planets { get; set; }
		public List<Edge> Connections { get; set; }
	}
}
