using System.Collections.Generic;

namespace SharedLibrary.Models
{
	public class SessionMap
	{
		public int Id { get; set; }

		public int HeroId { get; set; }
		public Hero? Hero { get; set; }
	}
}
