using System;
using System.Runtime.InteropServices;
using BaseballApi.Contracts;
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
    [InlineData("Test", "Test Batter 1", "Test Batter 2", "Test Batter 3", "Test Pitcher 1", "Test Pitcher 2", "Test Bench Player", "Test City Testers", "New Tester Town Tubes", "St. Test Guinea Pigs")]
    [InlineData("Batter", "Test Batter 1", "Test Batter 2", "Test Batter 3")]
    [InlineData("Pitcher", "Test Pitcher 1", "Test Pitcher 2")]
    [InlineData("Test Batter 1", "Test Batter 1")]
    [InlineData("test batter 1", "Test Batter 1")]
    [InlineData("Test Pitcher 1", "Test Pitcher 1")]
    [InlineData("Test City Testers", "Test City Testers")]
    [InlineData("1", "Test Batter 1", "Test Pitcher 1")]
    [InlineData("2", "Test Batter 2", "Test Pitcher 2")]
    [InlineData("Mike Trout")]
    [InlineData("alternate", "St. Test Guinea Pigs")] // Example for searching by alternate team names
    [InlineData("tct", "Test City Testers")] // Example for searching by team abbreviation
    public async void TestSearch(string searchQuery, params string[] expectedNames)
    {
        var result = await Controller.Search(searchQuery);
        Assert.NotNull(result.Value);

        Assert.Equal(expectedNames.Length, result.Value.Count());
        foreach (var name in expectedNames)
        {
            var found = result.Value.FirstOrDefault(r => r.Name == name);
            Assert.Equal(name, found.Name);
            Assert.NotNull(found.Description);
            Assert.NotEqual(0, found.Id);
        }
    }

    [Theory]
    [InlineData("Test Batter 1", "TCT '22-'23")]
    [InlineData("Test Pitcher 1", "TCT '22-'23")]
    [InlineData("Test Batter 3", "NTT '22-'23")]
    [InlineData("Test Bench Player", "No Games")]
    public async void TestPlayerSearch(string playerName, string expectedDescription)
    {
        var result = await Controller.Search(playerName);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        var playerResult = result.Value.FirstOrDefault(r => r.Name == playerName);
        Assert.Equal(expectedDescription, playerResult.Description);
        Assert.Equal(SearchResultType.Player, playerResult.Type);
        Assert.NotEqual(0, playerResult.Id);
    }

    [Theory]
    [InlineData("Test City Testers", "TCT")]
    [InlineData("New Tester Town Tubes", "NTT")]
    public async void TestTeamSearch(string teamName, string expectedDescription)
    {
        var result = await Controller.Search(teamName);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        var teamResult = result.Value.FirstOrDefault(r => r.Name == teamName);
        Assert.Equal(expectedDescription, teamResult.Description);
        Assert.Equal(SearchResultType.Team, teamResult.Type);
        Assert.NotEqual(0, teamResult.Id);
    }
}
