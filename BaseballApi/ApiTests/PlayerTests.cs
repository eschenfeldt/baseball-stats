using BaseballApi.Contracts;
using BaseballApi.Controllers;
using BaseballApi.Import;
using BaseballApi.Integrations;
using BaseballApi.Models;
using BaseballApiTests.Mocks;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public class PlayerTests : BaseballTests
{
    private PlayerController Controller { get; }
    private LeaderboardController LeaderController { get; }
    private TestGameManager GameManager { get; }

    public PlayerTests(TestDatabaseFixture fixture, TestFileLoggerFixture fileLoggerFixture) : base(fixture, fileLoggerFixture)
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

    /// <summary>
    /// Simulated history of three players with the same name:
    /// - in 2022, BN4 wears #15 on Team 2 in a game scored in the DB
    /// - in 2022, BN5 wears #15 on Team 1 in a game scored in the DB
    /// - in <current year>, BN4 wears #26 on Team 1 with no game scored for that team yet
    /// - in <current year>, BN5 is retired and does not appear
    /// - in <current year>, a new player not in the db yet wears #15 on Team 1
    /// In the current year we use reference data to identify the player, which is the more common case.
    /// The scenarios where we pass the year are validating the setup of the test data.
    /// </summary>
    [Theory]
    [InlineData(4, 2, 15, 2022)]
    [InlineData(4, 1, 26, null)]
    [InlineData(5, 1, 15, 2022)]
    [InlineData(null, 1, 15, null)] // new player on the same team as 5 with same name
    public void TestIdentifyAmbiguousPlayer(int? expectedBatterNumber, long teamId, int number, int? year)
    {
        string playerName = "Ambiguous Player";
        var manager = new PlayerManager(Context);
        var player = manager.GetOrCreatePlayer(playerName, teamId, number, year);
        if (expectedBatterNumber == null)
        {
            // new player should have been created
            Assert.Equal(default, player.Id);
        }
        else
        {
            var expectedPlayerId = GameManager.GetPlayerId(expectedBatterNumber);
            Assert.Equal(expectedPlayerId, player.Id);
        }
    }

    /// <summary>
    /// Make sure we can still identify a player by name only if they're unambiguous in the DB.
    /// </summary>
    [Theory]
    [InlineData("Test Batter 1", 1)]
    [InlineData("Test Batter 2", 2)]
    [InlineData("Test Batter 3", 3)]
    [InlineData("New Player", null)]

    public void TestIdentifyUnambiguousPlayer(string playerName, int? expectedBatterNumber)
    {
        var manager = new PlayerManager(Context);
        // pick an arbitrary team and number to validate fallback logic when reference data is missing
        long teamId = Context.Teams.First().Id;
        int number = 999;
        var player = manager.GetOrCreatePlayer(playerName, teamId, number, null);
        if (expectedBatterNumber == null)
        {
            // new player should have been created
            Assert.Equal(default, player.Id);
        }
        else
        {
            var expectedPlayerId = GameManager.GetPlayerId(expectedBatterNumber);
            Assert.Equal(expectedPlayerId, player.Id);
        }
    }

    [Theory]
    [InlineData(19197, "Will Smith", "1995-03-28")] // Will Smith (C)
    [InlineData(8048, "Will Smith", "1989-07-10")]  // Will Smith (P)
    [InlineData(19755, "Shohei Ohtani")]
    [InlineData(10155, "Mike Trout")]
    [InlineData(10155, "Mike Trout", null, false)]
    [InlineData(33829, "Shōta Imanaga")]
    public async Task TestFindFangraphsPage(int expectedFangraphsId, string playerName, string? dateOfBirth = null, bool savePlayer = true)
    {
        var manager = new PlayerManager(Context);
        DateOnly? dob = dateOfBirth != null ? DateOnly.Parse(dateOfBirth) : null;
        var player = new Player { Name = playerName, DateOfBirth = dob };
        if (savePlayer)
        {
            // save player and reload to populate NameNormalized computed colum
            Context.Players.Add(player);
            await Context.SaveChangesAsync();
            Context.Entry(player).Reload();
        }
        try
        {
            var fangraphsPage = await manager.FindFangraphsPageForPlayer(player, CancellationToken.None);
            Assert.NotNull(fangraphsPage);
            var idString = fangraphsPage.Segments[3].Trim('/');
            var actualFangraphsId = int.Parse(idString);
            Assert.Equal(expectedFangraphsId, actualFangraphsId);
        }
        finally
        {
            if (savePlayer)
            {
                Context.Players.Remove(player);
                await Context.SaveChangesAsync();
            }
        }
    }

    [Fact]
    public async Task TestGetMLBAMPeople()
    {
        var manager = new MLBAMConnector();
        var response = await manager.GetPeopleAsync(new DateOnly(2024, 1, 1));
        Assert.NotEmpty(response.People);

        var willSmiths = response.People.Where(p => p.FullName == "Will Smith").ToList();
        Assert.True(willSmiths.Count >= 2);
        var catcherWillSmith = willSmiths.FirstOrDefault(p => p.Id == 669257);
        var pitcherWillSmith = willSmiths.FirstOrDefault(p => p.Id == 519293);
        Assert.Equal("1995-03-28", catcherWillSmith.BirthDate);
        Assert.Equal("1989-07-10", pitcherWillSmith.BirthDate);
    }

    [Fact]
    public async Task TestMLBAMReferencePlayerUpdate()
    {
        var logger = FileLoggerFactory.CreateLogger<ReferenceManager>();
        // update teams so the MLBAM IDs are present
        var realConnector = new MLBAMConnector();
        var realReferenceManager = new ReferenceManager(logger, Context, realConnector);
        var teamsUpdated = await realReferenceManager.UpdateTeamReferences(CancellationToken.None);

        var connector = new MockMLBAMConnector();
        var referenceManager = new ReferenceManager(logger, Context, connector);
        var result = await referenceManager.UpdatePlayerReferences(CancellationToken.None);
        Assert.Equal(1, result.CreatedCount);
        Assert.Equal(2, result.UpdatedCount);

        var dodgers = Context.Teams.First(t => t.Abbreviation == "LAD");

        var willSmithReference = Context.ReferencePlayers.First(rp => rp.Name == "Will Smith");
        // fetch will smith from db by fangraphs page because that's what we set in the test db setup
        var willSmithPlayer = Context.Players.First(p => p.Name == "Will Smith" && p.FangraphsPage == new Uri("https://www.fangraphs.com/players/will-smith/19197/stats?position=C"));
        Assert.Equal(669257, willSmithReference.MLBAMId);
        Assert.Equal(willSmithPlayer.Id, willSmithReference.PlayerId);
        Assert.Equal(dodgers.Id, willSmithReference.CurrentTeamId);
        Assert.Equal(16, willSmithReference.CurrentNumber);
        Assert.Equal(new DateOnly(1995, 3, 28), willSmithPlayer.DateOfBirth);

        var ohtaniReference = Context.ReferencePlayers.First(rp => rp.Name == "Shohei Ohtani");
        Assert.Equal(660271, ohtaniReference.MLBAMId);
        Assert.Null(ohtaniReference.PlayerId); // Ohtani is not in the test db as a scored player
        Assert.Equal(dodgers.Id, ohtaniReference.CurrentTeamId);
        Assert.Equal(17, ohtaniReference.CurrentNumber);
        Assert.Equal(new DateOnly(1994, 7, 5), ohtaniReference.DateOfBirth);

        var cubs = Context.Teams.First(t => t.Abbreviation == "CHC");

        var shotaReference = Context.ReferencePlayers.First(rp => rp.Name == "Shota Imanaga");
        Assert.Equal(684007, shotaReference.MLBAMId);
        var shotaPlayer = Context.Players.First(p => p.Name == "Shōta Imanaga");
        Assert.Equal(shotaPlayer.Id, shotaReference.PlayerId);
        Assert.Equal(cubs.Id, shotaReference.CurrentTeamId);
        Assert.Equal(18, shotaReference.CurrentNumber);
        Assert.Equal(new DateOnly(1993, 9, 1), shotaReference.DateOfBirth);
        Assert.Equal("shota imanaga", shotaPlayer.NameNormalized);

        var secondUpdateResults = await referenceManager.UpdatePlayerReferences(CancellationToken.None);
        Assert.Equal(0, secondUpdateResults.CreatedCount);
        Assert.Equal(0, secondUpdateResults.UpdatedCount);
    }

    [Fact]
    public async Task TestFangraphsLinkUpdater()
    {
        // first make sure MLBAM references are up to date
        var logger = FileLoggerFactory.CreateLogger<ReferenceManager>();
        var realConnector = new MLBAMConnector();
        var realReferenceManager = new ReferenceManager(logger, Context, realConnector);
        var teamsUpdated = await realReferenceManager.UpdateTeamReferences(CancellationToken.None);

        var connector = new MockMLBAMConnector();
        var referenceManager = new ReferenceManager(logger, Context, connector);
        var result = await referenceManager.UpdatePlayerReferences(CancellationToken.None);

        var dodgers = Context.Teams.First(t => t.Abbreviation == "LAD");

        // now fill in missing fangraphs links and IDs
        var playerManager = new PlayerManager(Context);
        var updateResult = await referenceManager.UpdateFangraphsLinks(CancellationToken.None);
        Assert.Equal(1, updateResult.PlayersUpdated); // only Shōta Imanaga is missing a link in the test data
        Assert.Equal(2, updateResult.ReferencePlayersUpdated); // Will Smith and Shota Imanaga reference players are missing IDs

        var shotaPlayer = Context.Players.First(p => p.Name == "Shōta Imanaga");
        Assert.Equal(new Uri("https://www.fangraphs.com/players/shota-imanaga/33829/stats/pitching"), shotaPlayer.FangraphsPage);
        var shotaReference = Context.ReferencePlayers.First(rp => rp.Name == "Shota Imanaga");
        Assert.Equal("33829", shotaReference.FangraphsId);
        var willSmithPlayer = Context.Players.FirstOrDefault(p => p.Name == "Will Smith" && p.FangraphsPage == new Uri("https://www.fangraphs.com/players/will-smith/19197/stats?position=C"));
        Assert.NotNull(willSmithPlayer);
        var willSmithReference = Context.ReferencePlayers.First(rp => rp.Name == "Will Smith");
        Assert.Equal("19197", willSmithReference.FangraphsId);

        // running again should find no updates
        var secondUpdateResult = await referenceManager.UpdateFangraphsLinks(CancellationToken.None);
        Assert.Equal(0, secondUpdateResult.PlayersUpdated);
        Assert.Equal(0, secondUpdateResult.ReferencePlayersUpdated);
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