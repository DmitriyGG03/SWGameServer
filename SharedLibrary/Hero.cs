namespace SharedLibrary;

public class Hero
{
    public int Id { get; set; }
	public string Name { get; set; }
    public int Resourses { get; set; }
    public byte ResearchShipLimit { get; set; } 
    public byte ColonizationShipLimit { get; set; }

	//TODO: Create class Planet and implement one to many behavior
	public int UserId { get; set; }
	public User? User { get; set; }
}