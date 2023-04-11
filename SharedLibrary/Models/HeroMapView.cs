namespace SharedLibrary.Models
{
	public class HeroMapView
	{
		public int Id { get; set; }

		public int HeroId { get; set; }
		public Hero? Hero { get; set; }
	}
}
