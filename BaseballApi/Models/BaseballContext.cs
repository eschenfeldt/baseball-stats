using Microsoft.EntityFrameworkCore;
using BaseballApi;

namespace BaseballApi.Models;

public class BaseballContext : DbContext
{
    public BaseballContext(DbContextOptions<BaseballContext> options) : base(options) { }

    public DbSet<Game> Games { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Park> Parks { get; set; }
    public DbSet<Player> Players { get; set; }

    public DbSet<BoxScore> BoxScores { get; set; }
    public DbSet<Batter> Batters { get; set; }
    public DbSet<Fielder> Fielders { get; set; }
    public DbSet<Pitcher> Pitchers { get; set; }

    public DbSet<AlternateParkName> AlternateParkNames { get; set; }
    public DbSet<AlternateTeamName> AlternateTeamNames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Game>()
            .HasIndex(g => g.ExternalId)
            .IsUnique()
            .HasFilter("\"ExternalId\" != '00000000-0000-0000-0000-000000000000'"); // allow multiple games with default external id
    }
}