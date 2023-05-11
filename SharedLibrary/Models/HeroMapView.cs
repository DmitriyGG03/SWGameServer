using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
	[Table("HeroMaps"), Serializable]
	public class HeroMapView
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public Guid Id { get; set; }
		[ForeignKey(nameof(Hero))]
		public Guid HeroId { get; set; }
		public Hero? Hero { get; set; }
		
		[ForeignKey(nameof(HomePlanet))]
		public Guid HomePlanetId { get; set; }
		public Planet HomePlanet { get; set; }
		
		public List<Planet> Planets { get; set; }
		public List<Edge> Connections { get; set; }
	}
}
