using System;
using BaseballApi.Controllers;
using BaseballApi.Models;
using BaseballApi.Import;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BaseballApiTests;

public class MediaTests : BaseballTests
{
    MediaController Controller { get; }
    TestGameManager TestGameManager { get; }

    public MediaTests(TestDatabaseFixture fixture) : base(fixture)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<TestDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        RemoteFileManager remoteFileManager = new(configuration, nameof(MediaTests));
        Controller = new MediaController(Context, remoteFileManager);
        TestGameManager = new TestGameManager(Context);
    }

    public static TheoryData<int?, int?, int?, List<MockFile>> Thumbnails => new()
    {
        {null, null, null, []},
        // {4, 4, null, []}
    };

    [Theory]
    [MemberData(nameof(Thumbnails))]
    public async Task TestGetThumbnails(int? gameNumber, int? batterNumber, int? pitcherNumber, List<MockFile> expectedFiles)
    {
        long? gameId = null;
        if (gameNumber.HasValue)
        {
            gameId = TestGameManager.GetGameId(Context, gameNumber.Value);
        }
        long? playerId = null;
        if (batterNumber.HasValue || pitcherNumber.HasValue)
        {
            playerId = TestGameManager.GetPlayerId(batterNumber, pitcherNumber);
        }
        var result = await Controller.GetThumbnails(gameId: gameId, playerId: playerId);
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.All(result.Value.Results, (actual, index) =>
        {
            var expected = expectedFiles[index];
            Assert.Equal(expected.AssetIdentifier, actual.AssetIdentifier);
            Assert.Equal(expected.ExpectedKey, actual.Key);
            Assert.Equal(expected.DateTime, actual.DateTime);
        });
    }

    [Fact]
    public async Task TestImportLivePhotos()
    {
        var gameId = TestGameManager.GetGameId(Context, 1);

        var mediaBefore = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaBefore);
        Assert.NotNull(mediaBefore.Value);
        var countBefore = mediaBefore.Value.TotalCount;

        // now import two live photos to the game
        var files = new List<IFormFile>();
        var livePhotoDirectory = Path.Join("data", "media", "live photos");
        foreach (var filePath in Directory.EnumerateFiles(livePhotoDirectory))
        {
            var fileName = Path.GetFileName(filePath);
            using var fileStream = File.OpenRead(filePath);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName);
            files.Add(formFile);
        }

        await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));

        var mediaAfter = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaAfter);
        Assert.NotNull(mediaAfter.Value);
        var countAfter = mediaAfter.Value.TotalCount;
        Assert.Equal(countBefore + 2, countAfter);

        Assert.Fail("Add better assertions here.");
    }

}

public struct MockResource
{
    public long? GameId { get; set; }
    public long? PlayerId { get; set; }
    public required Guid AssetIdentifier { get; set; }
    public MockFile? OriginalPhoto { get; set; }
    public MockFile? OriginalVideo { get; set; }
    public MockFile Thumbnail { get; set; }
}

public struct MockFile
{
    public required Guid AssetIdentifier { get; set; }
    public required DateTimeOffset DateTime { get; set; }
    public required RemoteFilePurpose Purpose { get; set; }
    public string? NameModifier { get; set; }
    public string Extension { get; set; }
    public string OriginalFileName { get; set; }
    public string ExpectedKey { get; set; }
}
