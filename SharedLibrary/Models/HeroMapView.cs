using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
		
		[NotMapped]
		public Dictionary<Guid, byte> ResearchedPlanetsWithDaysNumber
		{
			get
			{
				return Planets.Where(x => x.Status == (int)PlanetStatus.Researched)
					.Select(x => new {x.Id, x.DaysNumber})
					.ToDictionary(x => x.Id, v => v.DaysNumber);
			}
		}
		[NotMapped]
		public Dictionary<Guid, byte> ColonizedPlanetsWithDaysNumber
		{
			get
			{
				return Planets.Where(x => x.Status == (int)PlanetStatus.Colonized)
					.Select(x => new {x.Id, x.DaysNumber})
					.ToDictionary(x => x.Id, v => v.DaysNumber);
			}
		}
		
		[NotMapped]
		public ICollection<Planet> KnownPlanets
		{
			get
			{
				return Planets.Where(x => x.Status == (int)PlanetStatus.Known).ToList();
			}
		}
		
		[NotMapped]
		public ICollection<Planet> ColonizedPlanets
		{
			get
			{
				return Planets.Where(x => x.Status == (int)PlanetStatus.Colonized).ToList();
			}
		}
		
		[NotMapped]
		public ICollection<Planet> ResearchedPlanets
		{
			get
			{
				return Planets.Where(x => x.Status == (int)PlanetStatus.Researched).ToList();
			}
		}
	}
}
