using System.ComponentModel;
using System.Linq.Expressions;
using BaseballApi;
using BaseballApi.Contracts;
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

    static readonly string Batter1Name = "Test Batter 1";
    static readonly string Batter2Name = "Test Batter 2";
    static readonly string Batter3Name = "Test Batter 3";
    static readonly Func<LeaderboardPlayer, int> GetGames = (lb) => lb.Games;
    static readonly Func<LeaderboardPlayer, int> GetAb = (lb) => lb.AtBats;
    static readonly Func<LeaderboardPlayer, int> GetH = (lb) => lb.Hits;
    static readonly Func<LeaderboardPlayer, decimal?> GetBattingAverage = (lb) =>
    {
        if (lb.BattingAverage.HasValue)
        {
            return decimal.Round(lb.BattingAverage.Value, 3);
        }
        else
        {
            return null;
        }
    };

    public static TheoryData<Func<LeaderboardPlayer, decimal?>, string, int?, decimal?> DecimalStats => new()
    {
        { GetBattingAverage, Batter1Name, null, 0.333M },
        { GetBattingAverage, Batter1Name, 2022, 0.333M },
        { GetBattingAverage, Batter1Name, 2023, 0.333M },
        { GetBattingAverage, Batter2Name, null, 0.333M },
        { GetBattingAverage, Batter2Name, 2022, 0.333M },
        { GetBattingAverage, Batter3Name, null, 0.273M },
        { GetBattingAverage, Batter3Name, 2022, 0.250M },
        { GetBattingAverage, Batter3Name, 2023, 0.333M },
    };

    public static TheoryData<Func<LeaderboardPlayer, int>, string, int?, int> IntegerStats => new()
    {
        { GetGames, Batter1Name, null, 2 },
        { GetGames, Batter1Name, 2022, 1 },
        { GetGames, Batter3Name, null, 3 },
        { GetAb, Batter1Name, null, 6}
    };

    [Theory]
    [MemberData(nameof(DecimalStats))]
    public async Task TestDecimalStat(Func<LeaderboardPlayer, decimal?> selectValue, string name, int? year, decimal? expected)
    {
        var player = await GetBattingLeader(name, year);
        Assert.Equal(name, player.Player.Name);
        var actualValue = selectValue(player);
        Assert.Equal(expected, actualValue);
    }

    [Theory]
    [MemberData(nameof(IntegerStats))]
    public async Task TestIntegerStat(Func<LeaderboardPlayer, int> selectValue, string name, int? year, int expected)
    {
        var player = await GetBattingLeader(name, year);
        Assert.Equal(name, player.Player.Name);
        Assert.Equal(expected, selectValue(player));
    }

    private async Task<LeaderboardPlayer> GetBattingLeader(string name, int? year)
    {
        var leaders = await LeaderController.GetBattingLeaders(new BatterLeaderboardParams
        {
            Skip = 0,
            Take = 10,
            Year = year,
            MinPlateAppearances = 0
        });
        Assert.NotNull(leaders.Value);
        var player = leaders.Value.Results.FirstOrDefault(l => l.Player.Name == name);
        Assert.NotNull(player);
        return player;
    }
}