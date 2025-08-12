using BaseballApi.Contracts;
using BaseballApi.Controllers;
using BaseballApi.Import;
using Microsoft.Extensions.Configuration;

namespace BaseballApiTests;

public class GameTests : BaseballTests
{
    private GamesController Controller { get; }
    private TestGameManager TestGameManager { get; }
    public GameTests(TestDatabaseFixture fixture) : base(fixture)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<TestDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        RemoteFileManager remoteFileManager = new(configuration, nameof(GameTests));
        Controller = new GamesController(Context, remoteFileManager);
        TestGameManager = new TestGameManager(Context);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    // intentionally not validating games 4+ since they don't have player info
    public async void TestGetGames(int testGameNumber, int expectedSkip)
    {
        var games = await Controller.GetGames(expectedSkip, 1, asc: true);
        Assert.NotNull(games.Value);
        Assert.Equal(TestGameManager.GameCount, games.Value.TotalCount);
        Assert.Single(games.Value.Results);
        var gameSummary = games.Value.Results.FirstOrDefault();
        TestGameManager.ValidateGameSummary(gameSummary, testGameNumber);

        var game = await Controller.GetGame(gameSummary.Id);
        Assert.NotNull(game);
        TestGameManager.ValidateGame(game.Value, testGameNumber);
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 3)]
    [InlineData(3, 1)]
    public async void TestGetGamesByTeam(int testTeamNumber, int gameCount)
    {
        var teamId = TestGameManager.GetTeamId(testTeamNumber);
        var games = await Controller.GetGames(0, 10, teamId: teamId);
        Assert.NotNull(games.Value);
        Assert.Equal(gameCount, games.Value.TotalCount);
    }


    [Theory]
    [InlineData(1, 4, 2, 1, 2)]
    [InlineData(2, 3, 0, 2, 2)]
    // [InlineData(3, 1, 1, 0, 1)] // Team 3 does not have any player data so the endpoint fails
    public async void TestGetTeamSummaryStats(int testTeamNumber, int games, int wins, int losses, int parks)
    {
        var teamId = TestGameManager.GetTeamId(testTeamNumber);
        var stats = await Controller.GetSummaryStats(teamId);

        void ValidateStat(decimal expected, Stat statDef)
        {
            Assert.NotNull(stats.Value);
            SummaryStat? stat = stats.Value.FirstOrDefault(s => s.Definition.Name == statDef.Name);
            Assert.NotNull(stat);
            Assert.Equal(expected, stat.Value.Value);
        }

        ValidateStat(games, Stat.Games);
        ValidateStat(parks, Stat.Parks);
        ValidateStat(wins, Stat.Wins);
        ValidateStat(losses, Stat.Losses);
    }

    [Theory]
    [InlineData("Test Park", 1, 2, 0, 1)]
    [InlineData("Test Stadium", 6, 5, 1, 4)]
    public async void TestGetParkSummaryStats(string parkName, int games, int teams, int wins, int losses)
    {
        var parkId = Context.Parks.FirstOrDefault(p => p.Name == parkName)?.Id;
        Assert.NotNull(parkId);
        var stats = await Controller.GetSummaryStats(parkId: parkId);

        void ValidateStat(decimal expected, Stat statDef)
        {
            Assert.NotNull(stats.Value);
            SummaryStat? stat = stats.Value.FirstOrDefault(s => s.Definition.Name == statDef.Name);
            Assert.NotNull(stat);
            Assert.Equal(expected, stat.Value.Value);
        }

        ValidateStat(games, Stat.Games);
        ValidateStat(teams, Stat.Teams);
        ValidateStat(wins, Stat.Wins);
        ValidateStat(losses, Stat.Losses);
    }

    [Theory]
    [InlineData(2022, 2)]
    [InlineData(2023, 1)]
    [InlineData(2024, 1)]
    public async void TestGetGamesByYear(int year, int gameCount)
    {
        var games = await Controller.GetGames(0, 10, year: year);
        Assert.NotNull(games.Value);
        Assert.Equal(gameCount, games.Value.TotalCount);
    }

    [Theory]
    [InlineData(null, 2022, 2023, 2024, 2025)]
    [InlineData(1, 2022, 2023, 2024)]
    [InlineData(2, 2022, 2023)]
    [InlineData(3, 2024)]
    public async void TestGetAvailableYears(int? testTeamNumber, params int[] years)
    {
        long? teamId = testTeamNumber.HasValue ? TestGameManager.GetTeamId(testTeamNumber.Value) : null;
        var actualYears = await Controller.GetGameYears(teamId);
        Assert.NotNull(actualYears.Value);
        Assert.Equal(years, actualYears.Value);
    }
}
