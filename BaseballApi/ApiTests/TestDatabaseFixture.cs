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
                    var testGameManager = new TestGameManager(context);
                    testGameManager.AddAllGames(context);
                }
                _dbInitialized = true;
            }
        }
    }

    void AddTeams(BaseballContext context)
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
            new Team { City = "Blankville", Name = "Blanks", Abbreviation = "BNK" }
        );
    }

    void AddPlayers(BaseballContext context)
    {
        context.AddRange(
            new Player { Name = "Test Pitcher 1" },
            new Player { Name = "Test Batter 1" },
            new Player { Name = "Test Pitcher 2" },
            new Player { Name = "Test Batter 2" },
            new Player { Name = "Test Batter 3" },
            new Player { Name = "Test Bench Player" }
        );
    }

    void AddLocations(BaseballContext context)
    {
        context.AddRange(
            new Park { Name = "Test Park" },
            new Park { Name = "Test Stadium" }
        );
    }

    void AddConstants(BaseballContext context)
    {
        context.AddRange(
            new FangraphsConstants { Year = 2024 },
            new FangraphsConstants { Year = 2023 },
            new FangraphsConstants { Year = 2022 }
        );
    }

    public BaseballContext CreateContext()
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
