using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedLibrary.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public string Salt { get; set; }
		public string Email { get; set; }

		public ICollection<Hero> Heroes { get; set; }
	}
}

