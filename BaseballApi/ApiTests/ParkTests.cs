using BaseballApi.Controllers;

namespace BaseballApiTests;

public class ParkTests : BaseballTests
{
    private ParkController Controller { get; }
    private TestGameManager TestGameManager { get; }

    public ParkTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new ParkController(Context);
        TestGameManager = new TestGameManager(Context);
    }

    [Theory]
    [InlineData("Test Park")]
    [InlineData("Test Stadium")]
    public async void TestGetParks(string name)
    {
        var parks = await Controller.GetParks();
        Assert.NotNull(parks.Value);
        var park = parks.Value.FirstOrDefault(p => p.Name == name);
        Assert.NotNull(park);
        Assert.Equal(name, park.Name);
    }

    [Theory]
    [InlineData("Test Stadium", null, 6, 5, 1, 4, "2022-04-27", "2025-06-30")]
    [InlineData("Test Park", null, 1, 2, 0, 1, "2023-06-27", "2023-06-27")]
    [InlineData("Test Stadium", 1, 3, 3, 1, 1, "2022-04-27", "2024-06-30")]
    [InlineData("Test Stadium", 2, 2, 2, 0, 1, "2022-04-27", "2022-08-27")]
    [InlineData("Test Park", 1, 1, 2, 0, 1, "2023-06-27", "2023-06-27")]
    [InlineData("Test Park", 2, 1, 2, 0, 1, "2023-06-27", "2023-06-27")]
    public async void TestGetParkSummaries(string name, int? teamNumber, int games, int teams, int wins, int losses, string firstGameDate, string lastGameDate)
    {
        long? teamId = teamNumber.HasValue ? TestGameManager.GetTeamId(teamNumber.Value) : null;
        var summaries = await Controller.GetParkSummaries(0, 10, teamId: teamId);
        Assert.NotNull(summaries.Value);
        var parkSummary = summaries.Value.Results.FirstOrDefault(p => p.Park.Name == name);
        Assert.NotNull(parkSummary);
        Assert.Equal(games, parkSummary.Games);
        Assert.Equal(teams, parkSummary.Teams);
        Assert.Equal(wins, parkSummary.Wins);
        Assert.Equal(losses, parkSummary.Losses);
        Assert.Equal(DateOnly.Parse(firstGameDate), parkSummary.FirstGameDate);
        Assert.Equal(DateOnly.Parse(lastGameDate), parkSummary.LastGameDate);
    }
}