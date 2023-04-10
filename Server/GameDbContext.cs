using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

namespace Server;

public class GameDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Hero> Heroes { get; set; }
    public DbSet<HeroMapView> HeroMapViews { get; set; }
    public DbSet<SessionMap> SessionMaps { get; set; }

    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}