using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SharedLibrary.Models
{
	[Serializable]
	public class HeroMapView
	{
		public Guid HeroId { get; set; }

		public List<Planet> Planets { get; set; }
		public List<Edge> Connections { get; set; }
		/*
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
		 */
	}
}
