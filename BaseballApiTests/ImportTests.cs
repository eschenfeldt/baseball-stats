using BaseballApi;
using BaseballApi.Contracts;
using BaseballApi.Controllers;
using BaseballApi.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BaseballApiTests;

public class ImportTests(TestImportDatabaseFixture fixture) : IClassFixture<TestImportDatabaseFixture>, IDisposable
{
    private TestImportDatabaseFixture Fixture { get; } = fixture;

    [Fact]
    public async void TestImportGameViaController()
    {
        using BaseballContext context = Fixture.CreateContext();
        var gamesController = new GamesController(context);
        var playerController = new PlayerController(context);
        var teamsController = new TeamsController(context);

        // add one of the teams and players from the test game before importingto be sure the importer doesn't duplicate them
        context.Teams.Add(new Team { City = "Washington", Name = "Nationals" });
        context.Players.Add(new Player { Name = "Willson Contreras" });
        await context.SaveChangesAsync();

        var teamsBefore = await teamsController.GetTeams();
        Assert.NotNull(teamsBefore.Value);
        Assert.Single(teamsBefore.Value);
        var playersBefore = await playerController.GetPlayers();
        Assert.NotNull(playersBefore.Value);
        Assert.Single(playersBefore.Value);

        // now import the test game
        var files = new List<IFormFile>();
        var testGameDir = Path.Join("data", "Game 1");
        foreach (var filePath in Directory.EnumerateFiles(testGameDir))
        {
            var fileName = Path.GetFileName(filePath);
            using var fileStream = File.OpenRead(filePath);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName);
            files.Add(formFile);
        }
        var scheduled = new DateTimeOffset(2024, 7, 8, 16, 5, 0, TimeSpan.FromHours(-4));
        var actual = new DateTimeOffset(2024, 7, 8, 16, 6, 0, TimeSpan.FromHours(-4));
        var end = new DateTimeOffset(2024, 7, 8, 18, 29, 0, TimeSpan.FromHours(-4));
        var metadata = new GameMetadata
        {
            ScheduledStart = scheduled,
            ActualStart = actual,
            End = end,
            Away = new Team
            {
                City = "St. Louis",
                Name = "Cardinals"
            },
            Home = new Team
            {
                City = "Washington",
                Name = "Nationals"
            }
        };

        await gamesController.ImportGame(files, JsonConvert.SerializeObject(metadata));

        async Task ValidateGameInDb(DateTimeOffset expectedActualStart)
        {
            var games = await gamesController.GetGames(0, 2);
            Assert.NotNull(games.Value);
            Assert.Equal(1, games.Value.TotalCount);
            Assert.Single(games.Value.Results);
            var gameSummary = games.Value.Results.First();
            Assert.Equal("7/8/24 St. Louis Cardinals at Washington Nationals", gameSummary.Name);
            Assert.Equal("St. Louis Cardinals", gameSummary.AwayTeamName);
            Assert.Equal("Washington Nationals", gameSummary.HomeTeamName);
            Assert.Equal(scheduled, gameSummary.ScheduledTime);
            Assert.Equal(expectedActualStart, gameSummary.StartTime);
            Assert.Equal(end, gameSummary.EndTime);
            Assert.Equal(6, gameSummary.AwayScore);
            Assert.Equal(0, gameSummary.HomeScore);

            var gameTask = await gamesController.GetGame(gameSummary.Id);
            Assert.NotNull(gameTask.Value);
            var game = gameTask.Value;
            Assert.Equal("7/8/24 St. Louis Cardinals at Washington Nationals", game.Name);
            Assert.Equal("St. Louis Cardinals", game.AwayTeamName);
            Assert.Equal("Washington Nationals", game.HomeTeamName);
            Assert.Equal(scheduled, game.ScheduledTime);
            Assert.Equal(expectedActualStart, game.StartTime);
            Assert.Equal(end, game.EndTime);
            Assert.Equal(6, game.AwayScore);
            Assert.Equal(0, game.HomeScore);

            // validate box scores
            Assert.NotNull(game.HomeBoxScore);
            Assert.NotNull(game.AwayBoxScore);

            var homeBatterRuns = game.HomeBoxScore.Value.Batters.Select(b => b.Runs).Sum();
            var awayBatterRuns = game.AwayBoxScore.Value.Batters.Select(b => b.Runs).Sum();
            var homePitcherRuns = game.HomeBoxScore.Value.Pitchers.Select(p => p.Runs).Sum();
            var awayPitcherRuns = game.AwayBoxScore.Value.Pitchers.Select(p => p.Runs).Sum();

            Assert.Equal(game.HomeScore, homeBatterRuns);
            Assert.Equal(game.HomeScore, awayPitcherRuns);
            Assert.Equal(game.AwayScore, awayBatterRuns);
            Assert.Equal(game.AwayScore, homePitcherRuns);

            Assert.Equal(11, game.HomeBoxScore.Value.Batters.Count);
            Assert.Equal(3, game.HomeBoxScore.Value.Pitchers.Count);
            Assert.Equal(11, game.HomeBoxScore.Value.Fielders.Count);
            Assert.Equal(9, game.AwayBoxScore.Value.Batters.Count);
            Assert.Equal(3, game.AwayBoxScore.Value.Pitchers.Count);
            Assert.Equal(11, game.AwayBoxScore.Value.Fielders.Count);
        }

        await ValidateGameInDb(actual);

        async Task ValidatePlayers()
        {
            var playersAfter = await playerController.GetPlayers();
            Assert.NotNull(playersAfter.Value);
            Assert.Equal(26, playersAfter.Value.Count());
        }
        await ValidatePlayers();

        async Task ValidateTeams()
        {
            var teamsAfter = await teamsController.GetTeams();
            Assert.NotNull(teamsAfter.Value);
            Assert.Collection(teamsAfter.Value, t =>
            {
                Assert.Equal("Washington", t.City);
                Assert.Equal("Nationals", t.Name);
            }, t =>
            {
                Assert.Equal("St. Louis", t.City);
                Assert.Equal("Cardinals", t.Name);
            });
        }
        await ValidateTeams();

        // Edit the metadata and upload again to be sure we update rather than inserting another game
        var newStartTime = new DateTimeOffset(2024, 7, 8, 16, 4, 0, TimeSpan.FromHours(-4));
        metadata.ActualStart = newStartTime;

        await gamesController.ImportGame(files, JsonConvert.SerializeObject(metadata));

        await ValidateGameInDb(newStartTime);
        await ValidateTeams();
        await ValidatePlayers();
    }



    public void Dispose()
    {
        Fixture.Dispose();
    }
}
