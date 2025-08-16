using BaseballApi;
using BaseballApi.Contracts;
using BaseballApi.Controllers;
using BaseballApi.Import;
using BaseballApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BaseballApiTests;

public class ImportTests(TestImportDatabaseFixture fixture) : IClassFixture<TestImportDatabaseFixture>
{
    protected TestImportDatabaseFixture Fixture { get; } = fixture;

    private static GameMetadata PrepareGameForImport(out List<IFormFile> files)
    {
        return BaseTestImportDatabaseFixture.PrepareGameForImport(out files);
    }

    [Fact]
    public async void TestImportGameViaController()
    {
        using BaseballContext context = Fixture.CreateContext();
        var builder = new ConfigurationBuilder()
            .AddJsonFile("/run/secrets/app_settings", optional: true)
            .AddUserSecrets<TestDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        RemoteFileManager remoteFileManager = new(configuration, nameof(ImportTests));
        var gamesController = new GamesController(context, remoteFileManager);
        var parkController = new ParkController(context);
        var playerController = new PlayerController(context);
        var teamsController = new TeamsController(context);

        var remoteValidator = new RemoteFileValidator(remoteFileManager);

        // add one of the teams and players from the test game before importing to be sure the importer doesn't duplicate them
        var park = new Park { Name = "Nationals Park", TimeZone = "Eastern Standard Time" };
        context.Parks.Add(park);
        context.Teams.Add(new Team { City = "Washington", Name = "Nationals", HomePark = park });
        context.Players.Add(new Player { Name = "Willson Contreras" });
        await context.SaveChangesAsync();
        Assert.NotEqual(0, park.Id);

        var parksBefore = await parkController.GetParks();
        Assert.NotNull(parksBefore.Value);
        Assert.Single(parksBefore.Value);
        var teamsBefore = await teamsController.GetTeams();
        Assert.NotNull(teamsBefore.Value);
        Assert.Single(teamsBefore.Value);
        var playersBefore = await playerController.GetPlayers();
        Assert.NotNull(playersBefore.Value);
        Assert.Single(playersBefore.Value);

        // now import the test game
        var metadata = PrepareGameForImport(out List<IFormFile> files);
        await gamesController.ImportGame(files, JsonConvert.SerializeObject(metadata));

        async Task<GameDetail> ValidateGameInDb(DateTimeOffset expectedActualStart)
        {
            var games = await gamesController.GetGames(0, 2);
            Assert.NotNull(games.Value);
            Assert.Equal(1, games.Value.TotalCount);
            Assert.Single(games.Value.Results);
            var gameSummary = games.Value.Results.First();
            Assert.Equal("7/8/24 St. Louis Cardinals at Washington Nationals", gameSummary.Name);
            Assert.Equal("St. Louis Cardinals", gameSummary.AwayTeamName);
            Assert.Equal("Washington Nationals", gameSummary.HomeTeamName);
            Assert.Equal(metadata.ScheduledStart, gameSummary.ScheduledTime);
            Assert.Equal(expectedActualStart, gameSummary.StartTime);
            Assert.Equal(metadata.End, gameSummary.EndTime);
            Assert.Equal(park.Name, gameSummary.Location?.Name);
            Assert.Equal(park.Id, gameSummary.Location?.Id);
            Assert.Equal(6, gameSummary.AwayScore);
            Assert.Equal(0, gameSummary.HomeScore);
            Assert.NotNull(gameSummary.WinningTeam);
            Assert.Equal("Cardinals", gameSummary.WinningTeam.Name);
            Assert.NotNull(gameSummary.LosingTeam);
            Assert.Equal("Nationals", gameSummary.LosingTeam.Name);

            var gameTask = await gamesController.GetGame(gameSummary.Id);
            Assert.NotNull(gameTask);
            var game = gameTask.Value;
            Assert.Equal("7/8/24 St. Louis Cardinals at Washington Nationals", game.Name);
            Assert.Equal("St. Louis Cardinals", game.AwayTeamName);
            Assert.Equal("Washington Nationals", game.HomeTeamName);
            Assert.Equal(metadata.ScheduledStart, game.ScheduledTime);
            Assert.Equal(expectedActualStart, game.StartTime);
            Assert.Equal(metadata.End, game.EndTime);
            Assert.Equal(park.Name, game.Location?.Name);
            Assert.Equal(park.Id, game.Location?.Id);
            Assert.Equal(6, game.AwayScore);
            Assert.Equal(0, game.HomeScore);
            Assert.NotNull(game.WinningTeam);
            Assert.Equal("Cardinals", game.WinningTeam.Name);
            Assert.NotNull(game.LosingTeam);
            Assert.Equal("Nationals", game.LosingTeam.Name);
            Assert.NotNull(game.Scorecard);
            var scorecardFile = game.Scorecard.Value.File;
            Assert.NotEqual(Guid.Empty, scorecardFile.AssetIdentifier);
            Assert.Equal(".pdf", scorecardFile.Extension);
            Assert.Equal("scorecard.pdf", scorecardFile.OriginalFileName);
            await remoteValidator.ValidateFileExists(scorecardFile, "application/pdf");

            // validate box scores
            Assert.NotNull(game.HomeBoxScore);
            Assert.NotNull(game.AwayBoxScore);

            var homeBatterRuns = game.HomeBoxScore.Value.Batters.Select(b => b.Stats[Stat.Runs.Name]).Sum();
            var awayBatterRuns = game.AwayBoxScore.Value.Batters.Select(b => b.Stats[Stat.Runs.Name]).Sum();
            var homePitcherRuns = game.HomeBoxScore.Value.Pitchers.Select(p => p.Stats[Stat.Runs.Name]).Sum();
            var awayPitcherRuns = game.AwayBoxScore.Value.Pitchers.Select(p => p.Stats[Stat.Runs.Name]).Sum();

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

            return game;
        }

        Assert.NotNull(metadata.ActualStart);
        var originalGameDetail = await ValidateGameInDb(metadata.ActualStart.Value);
        var originalScorecardFile = originalGameDetail.Scorecard!.Value.File;

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

        await remoteValidator.ValidateFileDeleted(originalScorecardFile);

        var newGameDetail = await ValidateGameInDb(newStartTime);
        await ValidateTeams();
        await ValidatePlayers();

        var scoreCard = newGameDetail.Scorecard;
        Assert.NotNull(scoreCard);
        var scoreCardResource = await context.Scorecards.FirstOrDefaultAsync(s => s.AssetIdentifier == scoreCard.Value.File.AssetIdentifier);
        Assert.NotNull(scoreCardResource);
        await remoteFileManager.DeleteResource(scoreCardResource);

        await remoteValidator.ValidateFileDeleted(scoreCard.Value.File);
    }
}
