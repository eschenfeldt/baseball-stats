using System;
using BaseballApi.Controllers;
using BaseballApi.Import;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BaseballApiTests;

public class TestMediaImportDatabaseFixture : BaseTestImportDatabaseFixture, IAsyncLifetime
{
    public TestMediaImportDatabaseFixture() : base(nameof(TestMediaImportDatabaseFixture)) { }

    public long GameId { get; private set; }

    public async Task InitializeAsync()
    {
        var context = CreateContext();
        var remoteFileManager = new RemoteFileManager(Configuration, nameof(TestMediaImportDatabaseFixture));
        // import a game; files will attach to this game and 
        // it will include a scorecard to be sure that doesn't get flagged by any of these processes
        var gamesController = new GamesController(context, remoteFileManager);
        var metadata = PrepareGameForImport(out List<IFormFile> files);
        var result = await gamesController.ImportGame(files, JsonConvert.SerializeObject(metadata));
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Value.Id);
        Assert.Equal(files.Count, result.Value.Count);
        Assert.NotEqual(0, result.Value.Size);
        GameId = result.Value.Id;
    }

    public Task DisposeAsync()
    {
        // Cleanup logic specific to media import tests can be added here
        return Task.CompletedTask;
    }
}
