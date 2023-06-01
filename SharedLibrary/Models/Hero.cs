using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace SharedLibrary.Models
{
	[Table("Heroes"), Serializable]
	public class Hero
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public Guid HeroId { get; set; }
		[Required]
		public string Name { get; set; }
		[Range(0, int.MaxValue)]
		public int Resourses { get; set; }
		[Range(0, byte.MaxValue)]
		public byte ResearchShipLimit { get; set; }

		[Range(0, byte.MaxValue)] 
		public byte AvailableResearchShips { get; set; } = 0;
		[Range(0, byte.MaxValue)]
		public byte ColonizationShipLimit { get; set; }

		[Range(0, byte.MaxValue)] 
		public byte AvailableColonizationShips { get; set; } = 0;
		public int ColorStatus { get; set; }
		
		[Range(0, int.MaxValue)]
		public int AvailableSoldiers { get; set; }
		[Range(0, int.MaxValue)]
		public int SoldiersLimit { get; set; }
		
		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public ApplicationUser? User { get; set; }
		
		[ForeignKey(nameof(HomePlanet))]
		public Guid HomePlanetId { get; set; }
		public Planet? HomePlanet { get; set; }
		
		[ForeignKey(nameof(Session))]
		public Guid? SessionId { get; set; }
		public Session? Session { get; set; }

		public void InitializeAvailableSoldiers()
		{
			AvailableSoldiers = SoldiersLimit;
		}
		public int SetSoldiersLimitBasedOnPlanetSize(int planetSize)
		{
			int coefficient = 0;
			if (planetSize < 10)
			{
				coefficient = 8;
			}
			else if (planetSize < 20)
			{
				coefficient = 6;
			}
			else
			{
				coefficient = 5;
			}

			SoldiersLimit = planetSize * coefficient;
			return SoldiersLimit;
		}
	}
}

