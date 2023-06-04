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
	}
}
