using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
	[Table("ApplicationUsers"), Serializable]
	public class ApplicationUser
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
		public Guid Id { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public string Salt { get; set; }
		public string Email { get; set; }

		public ICollection<LobbyInfo>? LobbyInfos { get; set; }
		public ICollection<Hero>? Heroes { get; set; }
	}
}

