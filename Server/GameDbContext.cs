using Microsoft.EntityFrameworkCore;
using Server.Migrations;
using SharedLibrary.Models;

namespace Server;

public class GameDbContext : DbContext
{
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Hero> Heroes { get; set; }
    public DbSet<HeroMapView> HeroMaps { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SessionMap> SessionMaps { get; set; }
    public DbSet<Lobby> Lobbies { get; set; }
    public DbSet<LobbyInfo> LobbyInfos { get; set; }
    
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}