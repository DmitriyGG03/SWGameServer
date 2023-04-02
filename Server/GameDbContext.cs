using Microsoft.EntityFrameworkCore;
using SharedLibrary;

namespace Server;

public class GameDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
}