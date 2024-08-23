using BaseballApi.Controllers;
using BaseballApi.Import;

namespace BaseballApiTests;

public class GameTests : BaseballTests
{
    private GamesController Controller { get; }
    private TestGameManager TestGameManager { get; }
    public GameTests(TestDatabaseFixture fixture) : base(fixture)
    {
        RemoteFileManager remoteFileManager = new(nameof(GameTests));
        Controller = new GamesController(Context, remoteFileManager);
        TestGameManager = new TestGameManager(Context);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    // intentionally not validating game 4 since it doesn't have player info
    public async void TestGetGames(int testGameNumber, int expectedSkip)
    {
        var games = await Controller.GetGames(expectedSkip, 1, asc: true);
        Assert.NotNull(games.Value);
        Assert.Equal(4, games.Value.TotalCount);
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
    [InlineData(null, 2022, 2023, 2024)]
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
