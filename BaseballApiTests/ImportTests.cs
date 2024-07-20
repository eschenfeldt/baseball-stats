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

        async Task ValidateGameInDb(GamesController gamesController, DateTimeOffset expectedActualStart)
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
            Assert.Equal("7/8/24 St. Louis Cardinals vs Washington Nationals", game.Name);
            Assert.Equal("St. Louis Cardinals", game.AwayTeamName);
            Assert.Equal("Washington Nationals", game.HomeTeamName);
            Assert.Equal(scheduled, game.ScheduledTime);
            Assert.Equal(expectedActualStart, game.StartTime);
            Assert.Equal(end, game.EndTime);
            Assert.Equal(6, game.AwayScore);
            Assert.Equal(0, game.HomeScore);
        }

        await ValidateGameInDb(gamesController, actual);

        // TODO: Add assertions about the box scores

        // TODO: Add assertions about players

        // TODO: Add assertions about teams

        // Edit the metadata and upload again to be sure we update rather than inserting another game
        var newStartTime = new DateTimeOffset(2024, 7, 8, 16, 4, 0, TimeSpan.FromHours(-4));
        metadata.ActualStart = newStartTime;

        await gamesController.ImportGame(files, JsonConvert.SerializeObject(metadata));

        await ValidateGameInDb(gamesController, newStartTime);
    }



    public void Dispose()
    {
        Fixture.Dispose();
    }
}
