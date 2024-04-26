using Microsoft.EntityFrameworkCore;
using BaseballApi;

namespace BaseballApi.Models;

public class BaseballContext : DbContext
{
    public BaseballContext(DbContextOptions<BaseballContext> options) : base(options) { }

    public required DbSet<Game> Games { get; set; }
    public required DbSet<Team> Teams { get; set; }
    public required DbSet<Park> Parks { get; set; }
    public required DbSet<Player> Players { get; set; }

    public required DbSet<BoxScore> BoxScores { get; set; }
    public required DbSet<Batter> Batters { get; set; }
    public required DbSet<Fielder> Fielders { get; set; }
    public required DbSet<Pitcher> Pitchers { get; set; }

    public required DbSet<AlternateParkName> AlternateParkNames { get; set; }
    public required DbSet<AlternateTeamName> AlternateTeamNames { get; set; }
}