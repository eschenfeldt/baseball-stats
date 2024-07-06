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
    public async void TestGetTeams(string city, string name)
    {
        var teams = await Controller.GetTeams();
        Assert.NotNull(teams.Value);
        var team = teams.Value.FirstOrDefault(t => t.City == city && t.Name == name);
        Assert.NotNull(team);
        Assert.Equal(city, team.City);
        Assert.Equal(name, team.Name);
    }
}