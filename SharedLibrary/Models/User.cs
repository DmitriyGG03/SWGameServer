using System.Collections.Generic;

namespace SharedLibrary.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public string Salt { get; set; }
		public string Email { get; set; }

		public ICollection<Hero> Heroes { get; set; }
	}
}

