using System;
using BaseballApi.Controllers;
using BaseballApi.Models;
using BaseballApi.Import;
using System.Security.Cryptography;

namespace BaseballApiTests;

public class MediaTests : BaseballTests
{
    MediaController Controller { get; }
    TestGameManager TestGameManager { get; }
    public MediaTests(TestDatabaseFixture fixture) : base(fixture)
    {
        Controller = new MediaController(Context, null);
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
            gameId = TestGameManager.GetGameId(gameNumber.Value);
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
}

public struct MockResource
{
    public int? GameId { get; set; }
    public int? PlayerId { get; set; }
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
