using BaseballApi;
using BaseballApi.Controllers;

namespace BaseballApiTests;

public class PlayerTests : BaseballTests
{
    private PlayerController Controller { get; }
    private LeaderboardController LeaderController { get; }

    public PlayerTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new PlayerController(Context);
        LeaderController = new LeaderboardController(Context);
    }

    [Theory]
    [InlineData("Test Batter 1")]
    [InlineData("Test Batter 2")]
    [InlineData("Test Batter 3")]
    [InlineData("Test Pitcher 1")]
    [InlineData("Test Pitcher 2")]
    public async void TestGetPlayers(string name)
    {
        var players = await Controller.GetPlayers();
        Assert.NotNull(players.Value);
        var player = players.Value.FirstOrDefault(p => p.Name == name);
        Assert.NotNull(player);
        Assert.Equal(name, player.Name);
    }

    public static TheoryData<string, int?, decimal?> BattingAverages => new()
    {
        { "Test Batter 1", null, 0.333M }
    };

    [Theory]
    [MemberData(nameof(BattingAverages))]
    public async void TestBattingAverage(string name, int? year, decimal? expected)
    {
        var player = await GetBattingLeader(name, year);
        Assert.Equal(name, player.Player.Name);
        Assert.Equal(expected, player.BattingAverage);
    }

    private async Task<LeaderboardBatter> GetBattingLeader(string name, int? year)
    {
        var leaders = await LeaderController.GetBattingLeaders(new LeaderboardParams
        {
            Skip = 0,
            Take = 10,
            Year = year
        });
        Assert.NotNull(leaders.Value);
        var player = leaders.Value.FirstOrDefault(l => l.Player.Name == name);
        Assert.NotNull(player);
        return player;
    }
}