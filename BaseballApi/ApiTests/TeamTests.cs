using BaseballApi.Contracts;
using BaseballApi.Controllers;

namespace BaseballApiTests;

public class TeamTests : BaseballTests
{
    private TeamsController Controller { get; }
    public TeamTests(TestDatabaseFixture fixture) : base(fixture)
    {
        this.Controller = new TeamsController(Context);
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
}