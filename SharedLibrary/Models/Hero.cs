﻿using System;
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
		public byte ColonizationShipLimit { get; set; }
		
		public int Argb { get; set; }
		[NotMapped]
		public Color Color { get => Color.FromArgb(Argb); }

		[ForeignKey(nameof(User))]
		public Guid UserId { get; set; }
		public ApplicationUser? User { get; set; }
		
		[ForeignKey(nameof(HeroMapView))]
		public Guid? HeroMapId { get; set; }
		public HeroMapView? HeroMapView { get; set; }
		
		[ForeignKey(nameof(Session))]
		public Guid? SessionId { get; set; }
		public Session? Session { get; set; }
	}
}

