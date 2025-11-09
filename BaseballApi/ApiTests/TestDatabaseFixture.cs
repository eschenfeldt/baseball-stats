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
                    AddConstants(context);
                    context.SaveChanges();
                    AddReferencePlayers(context);
                    context.SaveChanges();
                    var testGameManager = new TestGameManager(context);
                    testGameManager.AddAllGames(context);
                }
                _dbInitialized = true;
            }
        }
    }

    static void AddTeams(BaseballContext context)
    {
        context.AddRange(
            new Team { City = "Test City", Name = "Testers", Abbreviation = "TCT" },
            new Team { City = "New Tester Town", Name = "Tubes", Abbreviation = "NTT" },
            new Team
            {
                City = "St. Test",
                Name = "Guinea Pigs",
                Abbreviation = "STG",
                AlternateTeamNames = { new AlternateTeamName { FullName = "St. Test Alternates" } }
            },
            // teams with no specific tests, used for media import games
            new Team { City = "Dummyton", Name = "Dummies", Abbreviation = "DUM" },
            new Team { City = "Blankville", Name = "Blanks", Abbreviation = "BNK" },
            // A few relevant teams for Will Smith tests
            new Team { City = "Los Angeles", Name = "Dodgers", Abbreviation = "LAD" },
            new Team { City = "Atlanta", Name = "Braves", Abbreviation = "ATL" },
            new Team { City = "Texas", Name = "Rangers", Abbreviation = "TEX" },
            new Team { City = "Boston", Name = "Red Sox", Abbreviation = "BOS" }
        );
    }

    static void AddPlayers(BaseballContext context)
    {
        context.AddRange(
            new Player { Name = "Test Pitcher 1" },
            new Player { Name = "Test Batter 1" },
            new Player { Name = "Test Pitcher 2" },
            new Player { Name = "Test Batter 2" },
            new Player { Name = "Test Batter 3" },
            new Player { Name = "Test Bench Player" },
            new Player { Name = "Ambiguous Player", DateOfBirth = new DateOnly(1995, 5, 15) },
            new Player { Name = "Ambiguous Player", DateOfBirth = new DateOnly(1994, 6, 20) },
            // Dodgers Will Smith (C) and Atlanta Will Smith (P)
            new Player { Name = "Will Smith", FangraphsPage = new Uri("https://www.fangraphs.com/players/will-smith/19197/stats?position=C") },
            new Player { Name = "Will Smith", FangraphsPage = new Uri("https://www.fangraphs.com/players/will-smith/8048/stats?position=P") }
        );
    }

    static void AddReferencePlayers(BaseballContext context)
    {
        var team1 = context.Teams.First(t => t.City == "Test City");
        var batter4 = context.Players.First(p => p.Name == "Ambiguous Player" && p.DateOfBirth == new DateOnly(1995, 5, 15));
        context.AddRange(
            // Batter Number 1 without the reference link set correctly
            new ReferencePlayer { Name = "Test Batter 1", DateOfBirth = new DateOnly(1994, 6, 20) },
            // Batter Number 4
            new ReferencePlayer { Name = "Ambiguous Player", DateOfBirth = new DateOnly(1995, 5, 15), Player = batter4, CurrentTeam = team1, CurrentNumber = 26 },
            // Intentionally not adding Batter Number 5 to reference data
            // New batter not yet in DB
            new ReferencePlayer { Name = "Ambiguous Player", DateOfBirth = new DateOnly(2000, 1, 1), CurrentTeam = team1, CurrentNumber = 15 }
        );
    }

    static void AddLocations(BaseballContext context)
    {
        context.AddRange(
            new Park { Name = "Test Park" },
            new Park { Name = "Test Stadium" }
        );
    }

    static void AddConstants(BaseballContext context)
    {
        context.AddRange(
            new FangraphsConstants { Year = 2024 },
            new FangraphsConstants { Year = 2023 },
            new FangraphsConstants { Year = 2022 }
        );
    }

    public static BaseballContext CreateContext()
    {
        var configPath = Path.Join("/", "run", "secrets", "app_settings");
        var builder = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: true)
            .AddUserSecrets<TestDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        var ownerConnectionString = configuration["Baseball:OwnerConnectionString"];
        return new BaseballContext(new DbContextOptionsBuilder<BaseballContext>()
                                    .UseNpgsql(ownerConnectionString).Options);
    }
}
