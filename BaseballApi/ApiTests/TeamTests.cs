using BaseballApi.Controllers;
using BaseballApi.Integrations;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public class TeamTests : BaseballTests
{
    private TeamsController Controller { get; }
    public TeamTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new TeamsController(Context);
    }

    [Theory]
    [InlineData("Test City", "Testers")]
    [InlineData("New Tester Town", "Tubes")]
    [InlineData("St. Test", "Guinea Pigs")]
    public async void TestGetTeams(string city, string name)
    {
        var teams = await Controller.GetTeams();
        Assert.NotNull(teams.Value);
        var team = teams.Value.FirstOrDefault(t => t.City == city && t.Name == name);
        Assert.NotNull(team);
        Assert.Equal(city, team.City);
        Assert.Equal(name, team.Name);
    }

    [Theory]
    [InlineData("Test City", "Testers", 4, 2, 1, "2024-06-30", 2)]
    [InlineData("New Tester Town", "Tubes", 3, 0, 2, "2023-06-27", 2)]
    [InlineData("St. Test", "Guinea Pigs", 1, 1, 0, "2024-06-30", 1)]
    public void TestGetTeamSummaries(string city, string name, int games, int wins, int losses, string lastGameDate, int parks)
    {
        var teams = Controller.GetTeamSummaries(0, 10);
        Assert.NotNull(teams.Value);
        var team = teams.Value.Results.FirstOrDefault(t => t.Team.City == city && t.Team.Name == name);
        Assert.NotNull(team);
        Assert.Equal(wins, team.Wins);
        Assert.Equal(losses, team.Losses);
        Assert.Equal(games, team.Games);
        Assert.Equal(parks, team.Parks);
        var lastGame = DateOnly.Parse(lastGameDate);
        Assert.Equal(lastGame, team.LastGameDate);
    }

    [Fact]
    public async void TestGetMLBAMTeams()
    {
        var connector = new MLBAMConnector();
        var teams = await connector.GetTeamsAsync();
        Assert.True(teams.Teams.Count >= 30);
        var cubs = teams.Teams.FirstOrDefault(t => t.TeamName == "Cubs" && t.LocationName == "Chicago");
        Assert.Equal("CHC", cubs.Abbreviation);
    }

    [Fact]
    public async void TestUpdateTeamReferences()
    {
        var logger = FileLoggerFactory.CreateLogger<ReferenceManager>();
        var connector = new MLBAMConnector();
        var referenceManager = new ReferenceManager(logger, Context, connector);
        var updatedCount = await referenceManager.UpdateTeamReferences(CancellationToken.None);
        var rangers = Context.Teams.First(t => t.Abbreviation == "TEX");
        Assert.Equal(140, rangers.MLBAMId);
        var dodgers = Context.Teams.First(t => t.Abbreviation == "LAD");
        Assert.Equal(119, dodgers.MLBAMId);
        var atlanta = Context.Teams.First(t => t.Abbreviation == "ATL");
        Assert.Equal(144, atlanta.MLBAMId);
        var redSox = Context.Teams.First(t => t.Abbreviation == "BOS");
        Assert.Equal(111, redSox.MLBAMId);
        var cubs = Context.Teams.First(t => t.Abbreviation == "CHC");
        Assert.Equal(112, cubs.MLBAMId);
        Assert.Equal(5, updatedCount); // Only 5 real mlb teams in our test db
    }
}