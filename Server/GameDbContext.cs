using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Hero> Heroes { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SessionMap> SessionMaps { get; set; }
    public DbSet<Lobby> Lobbies { get; set; }
    public DbSet<Planet> Planets { get; set; }
    public DbSet<Edge> Connections { get; set; }
    public DbSet<HeroPlanetRelation> HeroPlanetRelations { get; set; }
    public DbSet<Battle> Battles { get; set; }
}