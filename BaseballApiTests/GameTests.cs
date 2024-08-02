using BaseballApi.Controllers;

namespace BaseballApiTests;

public class GameTests : BaseballTests
{
    private GamesController Controller { get; }
    private TestGameManager TestGameManager { get; }
    public GameTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new GamesController(Context);
        TestGameManager = new TestGameManager(Context);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 2)]
    [InlineData(3, 1)]
    // intentionally not validating game 4 since it doesn't have player info
    public async void TestGetGames(int testGameNumber, int expectedSkip)
    {
        var games = await Controller.GetGames(expectedSkip, 1);
        Assert.NotNull(games.Value);
        Assert.Equal(4, games.Value.TotalCount);
        Assert.Single(games.Value.Results);
        var gameSummary = games.Value.Results.FirstOrDefault();
        TestGameManager.ValidateGameSummary(gameSummary, testGameNumber);

        var game = await Controller.GetGame(gameSummary.Id);
        Assert.NotNull(game.Value);
        TestGameManager.ValidateGame(game.Value, testGameNumber);
    }

    [Theory]
    [InlineData(1, 4)]
    [InlineData(2, 3)]
    [InlineData(3, 1)]
    public async void TestGetGamesByTeam(int testTeamNumber, int gameCount)
    {
        var teamId = TestGameManager.GetTeamId(testTeamNumber);
        var games = await Controller.GetGames(0, 10, teamId);
        Assert.NotNull(games.Value);
        Assert.Equal(gameCount, games.Value.TotalCount);

    }
}
