using System;
using System.Runtime.InteropServices;
using MyApp.Namespace;

namespace BaseballApiTests;

public class SearchTests : BaseballTests
{
    private SearchController Controller { get; }

    public SearchTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new SearchController(Context);
    }

    [Theory]
    [InlineData("Test", "Test Batter 1", "Test Batter 2", "Test Batter 3", "Test Pitcher 1", "Test Pitcher 2", "Test City Testers", "New Tester Town Tubes", "St. Test Guinea Pigs")]
    [InlineData("Batter", "Test Batter 1", "Test Batter 2", "Test Batter 3")]
    [InlineData("Pitcher", "Test Pitcher 1", "Test Pitcher 2")]
    [InlineData("Team", "Test Team 1", "Test Team 2")]
    [InlineData("Test Batter 1", "Test Batter 1")]
    [InlineData("Test Pitcher 1", "Test Pitcher 1")]
    [InlineData("Test Team 1", "Test Team 1")]
    [InlineData("1", "Test Batter 1", "Test Pitcher 1", "Test Team 1")]
    [InlineData("2", "Test Batter 2", "Test Pitcher 2", "Test Team 2")]
    [InlineData("Mike Trout")]
    [InlineData("alternate", "St. Test Guninea Pigs")] // Example for searching by alternate team names
    [InlineData("tct", "Test City Testers")] // Example for searching by team abbreviation
    public async void TestSearch(string searchQuery, params string[] expectedNames)
    {
        var result = await Controller.Search(searchQuery);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        Assert.Equal(expectedNames.Length, result.Value.Count());
        foreach (var name in expectedNames)
        {
            var found = result.Value.FirstOrDefault(r => r.Name == name);
            Assert.Equal(name, found.Name);
            Assert.NotNull(found.Description);
            Assert.NotEqual(0, found.Id);
        }
    }
}
