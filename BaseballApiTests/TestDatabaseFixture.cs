using System.Net;
using BaseballApi;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseballApiTests;

public class TestDatabaseFixture
{
    private static readonly object _lock = new();
    private static bool _dbInitialized;

    public TestDatabaseFixture()
    {
        lock (_lock)
        {
            if (!_dbInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    AddTeams(context);
                    AddPlayers(context);
                    AddLocations(context);
                    context.SaveChanges();
                    AddGames(context);
                    context.SaveChanges();
                    AddGameDetails(context);
                }
                _dbInitialized = true;
            }
        }
    }

    void AddTeams(BaseballContext context)
    {
        context.AddRange(
            new Team { City = "Test City", Name = "Testers" },
            new Team { City = "New Tester Town", Name = "Tubes" }
        );
    }

    void AddPlayers(BaseballContext context)
    {
        context.AddRange(
            new Player { Name = "Test Pitcher 1" },
            new Player { Name = "Test Batter 1" },
            new Player { Name = "Test Pitcher 2" },
            new Player { Name = "Test Batter 2" },
            new Player { Name = "Test Batter 3" }
        );
    }

    void AddLocations(BaseballContext context)
    {
        context.AddRange(
            new Park { Name = "Test Park" },
            new Park { Name = "Test Stadium" }
        );
    }

    void AddGames(BaseballContext context)
    {
        var team1 = context.Teams.First(t => t.City == "Test City");
        var team2 = context.Teams.First(t => t.City == "New Tester Town");
        var homeBox = new BoxScore
        {
            Game = null,
            Team = team1
        };
        var awayBox = new BoxScore
        {
            Game = null,
            Team = team2
        };
        var game = new Game
        {
            Name = "2022 Test Game 1",
            Home = team1,
            HomeTeamName = "Test City Old Timers",
            Away = team2,
            AwayTeamName = "New Tester Town Tubes",
            BoxScores = [homeBox, awayBox]
        };
        homeBox.Game = game;
        context.AddRange(
            homeBox,
            awayBox,
            game
        );
    }

    /// <summary>
    /// Add *and save* detailed box scores for test games
    /// </summary>
    void AddGameDetails(BaseballContext context)
    {
        var manager = new TestGameManager(context);
        manager.AddBoxScore(1, true);
    }

    public BaseballContext CreateContext()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<TestDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        var ownerConnectionString = configuration["Baseball:OwnerConnectionString"];
        return new BaseballContext(new DbContextOptionsBuilder<BaseballContext>()
                                    .UseNpgsql(ownerConnectionString).Options);
    }
}
