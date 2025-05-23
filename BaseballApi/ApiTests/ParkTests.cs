using BaseballApi.Controllers;

namespace BaseballApiTests;

public class ParkTests : BaseballTests
{
    private ParkController Controller { get; }

    public ParkTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new ParkController(Context);
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
}