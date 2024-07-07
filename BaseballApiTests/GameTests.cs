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
    public async void TestGetGames(int testGameNumber, int expectedSkip)
    {
        var games = await Controller.GetGames(expectedSkip, 1);
        Assert.NotNull(games.Value);
        Assert.Single(games.Value);
        var gameSummary = games.Value.FirstOrDefault();

        var game = await Controller.GetGame(gameSummary.Id);
        Assert.NotNull(game.Value);
        TestGameManager.ValidateGame(game.Value, testGameNumber);
    }
}
