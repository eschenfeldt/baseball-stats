using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BaseballApi;
using BaseballApi.Contracts;
using BaseballApi.Controllers;
using BaseballApi.Import;
using BaseballApi.Models;

namespace BaseballApiTests;

public class PlayerTests : BaseballTests
{
    private PlayerController Controller { get; }
    private LeaderboardController LeaderController { get; }
    private TestGameManager GameManager { get; }

    public PlayerTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new PlayerController(Context);
        LeaderController = new LeaderboardController(Context);
        GameManager = new TestGameManager(Context);
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

    [Theory]
    [InlineData(4, 2, 2022)]
    [InlineData(4, 2, null)]
    [InlineData(5, 1, 2022)]
    [InlineData(5, 1, null)]
    public void TestIdentifyAmbiguousPlayer(int expectedBatterNumber, long teamId, int? year)
    {
        string playerName = "Ambiguous Player";
        var manager = new PlayerManager(Context);
        var player = manager.GetOrCreatePlayer(playerName, teamId, year ?? 2025);
        var expectedPlayerId = GameManager.GetPlayerId(expectedBatterNumber);
        Assert.Equal(expectedPlayerId, player.Id);
    }

    /// <summary>
    /// The point of this test is to be sure that if we have two players with the same name 
    ///  both registered in the db we can identify the correct one even if he's playing on a new team.
    /// Note that if we have only one of the players registered we're not going to know to disambiguate, 
    ///  so there will still be manual cleanup initially. TBD if that can be integrated on the web or if it will remain a DB process.
    /// </summary>
    [Theory]
    [InlineData(19197, "LAD", 2022)] // Will Smith (C)
    [InlineData(19197, "LAD", 2025)]
    [InlineData(8048, "ATL", 2021)]  // Will Smith (P)
    [InlineData(8048, "ATL", 2022)]  // played for two teams in 2022
    [InlineData(8048, "TEX", 2023)]
    public void TestIdentifyWillSmith(int expectedFangraphsId, string teamAbbreviation, int year)
    {
        string playerName = "Will Smith";
        long teamId = Context.Teams.Single(t => t.Abbreviation == teamAbbreviation).Id;
        var manager = new PlayerManager(Context);
        var player = manager.GetOrCreatePlayer(playerName, teamId, year);
        var idString = player.FangraphsPage?.Segments[3].Trim('/');
        Assert.NotNull(idString);
        var actualFangraphsId = int.Parse(idString);
        Assert.Equal(expectedFangraphsId, actualFangraphsId);
    }

    [Theory]
    [InlineData(19197, "Will Smith", "1995-03-28")] // Will Smith (C)
    [InlineData(8048, "Will Smith", "1989-07-10")]  // Will Smith (P)
    [InlineData(19755, "Shohei Ohtani")]
    [InlineData(10155, "Mike Trout")]
    public async Task TestFindFangraphsPage(int expectedFangraphsId, string playerName, string? dateOfBirth = null)
    {
        var manager = new PlayerManager(Context);
        DateOnly? dob = dateOfBirth != null ? DateOnly.Parse(dateOfBirth) : null;
        var player = new Player { Name = playerName, DateOfBirth = dob };
        var fangraphsPage = await manager.FindFangraphsPageForPlayer(player);
        Assert.NotNull(fangraphsPage);
        var idString = fangraphsPage.Segments[3].Trim('/');
        var actualFangraphsId = int.Parse(idString);
        Assert.Equal(expectedFangraphsId, actualFangraphsId);
    }

    static readonly string Batter1Name = "Test Batter 1";
    static readonly string Batter2Name = "Test Batter 2";
    static readonly string Batter3Name = "Test Batter 3";
    static readonly Func<LeaderboardPlayer, int> GetGames = (lb) => Convert.ToInt32(lb.Stats[Stat.Games.Name]);
    static readonly Func<LeaderboardPlayer, int> GetHr = (lb) => Convert.ToInt32(lb.Stats[Stat.Homeruns.Name]);
    static readonly Func<LeaderboardPlayer, decimal?> GetBattingAverage = (lb) =>
    {
        if (lb.Stats.TryGetValue(Stat.BattingAverage.Name, out decimal? ba) && ba.HasValue)
        {
            return decimal.Round(ba.Value, 3);
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
        { GetHr, Batter3Name, null, 2},
        { GetHr, Batter3Name, 2022, 1},
        { GetHr, Batter3Name, 2023, 1}
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
        var player = leaders.Value.Results.Single(l => l.Player.Name == name);
        return player;
    }
}