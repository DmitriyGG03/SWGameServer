using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
	[Table("ApplicationUsers")]
	public class ApplicationUser
	{
		[Key]
		public int Id { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public string Salt { get; set; }
		public string Email { get; set; }

		public ICollection<LobbyInfo>? LobbyInfos { get; set; }
		public ICollection<Hero>? Heroes { get; set; }
	}
}

