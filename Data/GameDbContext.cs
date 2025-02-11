using Microsoft.EntityFrameworkCore;
using GameLibraryAPI.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GameLibraryAPI.Data;

/// <summary>
/// GameDbContext class to represent database context for the application.
/// Provides access to Games table.
/// </summary>

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public GameDbContext() { } 

    public DbSet<Game> Games { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Load configuration from appsettings
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
