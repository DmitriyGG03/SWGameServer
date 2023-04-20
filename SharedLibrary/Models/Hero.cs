using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace SharedLibrary.Models
{
	public class Hero
	{
		[Key]
		public int HeroId { get; set; }
		[Required]
		public string Name { get; set; }
		[Range(0, int.MaxValue)]
		public int Resourses { get; set; }
		[Range(0, byte.MaxValue)]
		public byte ResearchShipLimit { get; set; }
		[Range(0, byte.MaxValue)]
		public byte ColonizationShipLimit { get; set; }
		
		public int Argb { get; set; }
		[NotMapped]
		public Color Color { get => Color.FromArgb(Argb); }

		[ForeignKey(nameof(User))]
		public int UserId { get; set; }
		public User? User { get; set; }
		
		[ForeignKey(nameof(HeroMap))]
		public int HeroMapId { get; set; }
		public HeroMap? HeroMap { get; set; }
		
		[ForeignKey(nameof(Session))]
		public Guid SessionId { get; set; }
		public Session? Session { get; set; }
	}
}

