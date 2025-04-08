using BaseballApi.Controllers;
using BaseballApi.Models;
using BaseballApi.Import;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using BaseballApi.Contracts;

namespace BaseballApiTests;

public class MediaTests : BaseballTests
{
    MediaController Controller { get; }
    RemoteFileManager RemoteFileManager { get; }
    RemoteFileValidator RemoteValidator { get; }
    TestGameManager TestGameManager { get; }

    public MediaTests(TestDatabaseFixture fixture) : base(fixture)
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<TestDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        RemoteFileManager = new(configuration, nameof(MediaTests));
        RemoteValidator = new(RemoteFileManager);
        Controller = new MediaController(Context, RemoteFileManager);
        TestGameManager = new TestGameManager(Context);
    }

    public static TheoryData<int?, int?, int?, List<MockFile>> Thumbnails => new()
    {
        {null, 1, null, []},
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
        var remoteValidator = new RemoteFileValidator(RemoteFileManager);
        var gameId = TestGameManager.GetGameId(Context, 1);

        var mediaBefore = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaBefore);
        Assert.NotNull(mediaBefore.Value);
        var countBefore = mediaBefore.Value.TotalCount;

        // now import two HEIC live photos to the game
        var files = new List<IFormFile>();
        var livePhotoDirectory = Path.Join("data", "media", "live photos");
        foreach (var filePath in EnumerateMediaFiles(livePhotoDirectory))
        {
            files.Add(CreateFormFile(filePath, out _));
        }

        await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));

        var mediaAfter = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaAfter);
        Assert.NotNull(mediaAfter.Value);
        var countAfter = mediaAfter.Value.TotalCount;
        Assert.Equal(countBefore + 2, countAfter);

        var file1 = mediaAfter.Value.Results.FirstOrDefault(x => x.OriginalFileName == "IMG_4762.HEIC");
        Assert.NotNull(file1.Key);
        var file2 = mediaAfter.Value.Results.FirstOrDefault(x => x.OriginalFileName == "IMG_4771.HEIC");
        Assert.NotNull(file2.Key);
        Assert.NotEqual(file1.AssetIdentifier, file2.AssetIdentifier);

        List<RemoteFileDetail> toBeDeleted = [];
        await ValidateLivePhoto(file1.AssetIdentifier, file1.OriginalFileName, toBeDeleted);
        await ValidateLivePhoto(file2.AssetIdentifier, file2.OriginalFileName, toBeDeleted);

        // now delete the resources and validate that the files are deleted from the remote
        var resource1 = await Context.MediaResources
            .FirstOrDefaultAsync(x => x.AssetIdentifier == file1.AssetIdentifier);
        Assert.NotNull(resource1);
        var resource2 = await Context.MediaResources
            .FirstOrDefaultAsync(x => x.AssetIdentifier == file2.AssetIdentifier);
        Assert.NotNull(resource2);
        await RemoteFileManager.DeleteResource(resource1);
        await RemoteFileManager.DeleteResource(resource2);

        foreach (var file in toBeDeleted)
        {
            await remoteValidator.ValidateFileDeleted(file);
        }
    }

    [Fact]
    public async Task TestImportVideo()
    {
        var remoteValidator = new RemoteFileValidator(RemoteFileManager);
        var gameId = TestGameManager.GetGameId(Context, 2);

        var mediaBefore = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaBefore);
        Assert.NotNull(mediaBefore.Value);
        var countBefore = mediaBefore.Value.TotalCount;

        // now import one hevc video to the game
        var files = new List<IFormFile>();
        var videoDirectory = Path.Join("data", "media", "video");
        foreach (var filePath in EnumerateMediaFiles(videoDirectory))
        {
            files.Add(CreateFormFile(filePath, out _));
        }

        await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));

        var mediaAfter = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaAfter);
        Assert.NotNull(mediaAfter.Value);
        var countAfter = mediaAfter.Value.TotalCount;
        Assert.Equal(countBefore + 1, countAfter);

        var file1 = mediaAfter.Value.Results.FirstOrDefault(x => x.OriginalFileName == "hevc.MOV");
        Assert.NotNull(file1.Key);

        List<RemoteFileDetail> toBeDeleted = [];

        await ValidateVideo(file1.AssetIdentifier, file1.OriginalFileName, toBeDeleted);

        // now delete the resources and validate that the files are deleted from the remote
        var resource1 = await Context.MediaResources
            .FirstOrDefaultAsync(x => x.AssetIdentifier == file1.AssetIdentifier);
        Assert.NotNull(resource1);
        await RemoteFileManager.DeleteResource(resource1);

        foreach (var file in toBeDeleted)
        {
            await remoteValidator.ValidateFileDeleted(file);
        }
    }

    [Fact]
    public async Task TestImportPhoto()
    {
        var remoteValidator = new RemoteFileValidator(RemoteFileManager);
        var gameId = TestGameManager.GetGameId(Context, 4);

        var mediaBefore = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaBefore);
        Assert.NotNull(mediaBefore.Value);
        var countBefore = mediaBefore.Value.TotalCount;

        var files = new List<IFormFile>();
        var photoDirectory = Path.Join("data", "media", "photos");
        Dictionary<string, MediaResourceType> resourceTypes = [];
        foreach (var filePath in EnumerateMediaFiles(photoDirectory))
        {
            files.Add(CreateFormFile(filePath, out _));
            resourceTypes[filePath] = MediaResourceType.Photo;
        }

        await this.ImportMedia(files, gameId, resourceTypes);

        var mediaAfter = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaAfter);
        Assert.NotNull(mediaAfter.Value);
        var countAfter = mediaAfter.Value.TotalCount;
        Assert.Equal(countBefore + 1, countAfter);

        var file1 = mediaAfter.Value.Results.FirstOrDefault(x => x.OriginalFileName == "IMG_4721.HEIC");
        Assert.NotNull(file1.Key);

        List<RemoteFileDetail> toBeDeleted = [];

        await ValidatePhoto(file1.AssetIdentifier, file1.OriginalFileName, toBeDeleted);

        // now delete the resources and validate that the files are deleted from the remote
        var resource1 = await Context.MediaResources
            .FirstOrDefaultAsync(x => x.AssetIdentifier == file1.AssetIdentifier);
        Assert.NotNull(resource1);
        await RemoteFileManager.DeleteResource(resource1);

        foreach (var file in toBeDeleted)
        {
            await remoteValidator.ValidateFileDeleted(file);
        }
    }

    [Fact]
    public async Task TestImportMedia()
    {
        var remoteValidator = new RemoteFileValidator(RemoteFileManager);
        var gameId = TestGameManager.GetGameId(Context, 3);

        var mediaBefore = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaBefore);
        Assert.NotNull(mediaBefore.Value);
        var countBefore = mediaBefore.Value.TotalCount;

        // now import several different media files to the game
        // this includes a video, two live photos, and a regular photo
        var files = new List<IFormFile>();
        var videoDirectory = Path.Join("data", "media", "video");
        var photoDirectory = Path.Join("data", "media", "photos");
        var livePhotoDirectory = Path.Join("data", "media", "live photos");
        Dictionary<string, MediaResourceType> resourceTypes = [];
        foreach (var filePath in EnumerateMediaFiles(photoDirectory))
        {
            files.Add(CreateFormFile(filePath, out string fileName));
            resourceTypes[fileName] = MediaResourceType.Photo;
        }
        foreach (var filePath in EnumerateMediaFiles(videoDirectory))
        {
            files.Add(CreateFormFile(filePath, out string fileName));
            resourceTypes[fileName] = MediaResourceType.Video;
        }
        foreach (var filePath in EnumerateMediaFiles(livePhotoDirectory))
        {
            files.Add(CreateFormFile(filePath, out string fileName));
            resourceTypes[fileName] = MediaResourceType.LivePhoto;
        }

        await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));

        var mediaAfter = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaAfter);
        Assert.NotNull(mediaAfter.Value);
        var countAfter = mediaAfter.Value.TotalCount;
        Assert.Equal(countBefore + 4, countAfter);

        List<RemoteFileDetail> toBeDeleted = [];
        foreach (var file in mediaAfter.Value.Results)
        {
            Assert.NotNull(file.Key);
            if (resourceTypes.TryGetValue(file.OriginalFileName, out MediaResourceType resourceType))
            {
                switch (resourceType)
                {
                    case MediaResourceType.Photo:
                        await ValidatePhoto(file.AssetIdentifier, file.OriginalFileName, toBeDeleted);
                        break;
                    case MediaResourceType.Video:
                        await ValidateVideo(file.AssetIdentifier, file.OriginalFileName, toBeDeleted);
                        break;
                    case MediaResourceType.LivePhoto:
                        await ValidateLivePhoto(file.AssetIdentifier, file.OriginalFileName, toBeDeleted);
                        break;
                    default:
                        Assert.Fail($"Unknown resource type {resourceType} for file {file.OriginalFileName}");
                        break;
                }
            }
            else
            {
                Assert.Fail($"No identified resource type for file {file.OriginalFileName}");
            }
        }

        foreach (var file in mediaAfter.Value.Results)
        {
            // now delete the resources and validate that the files are deleted from the remote
            var resource = await Context.MediaResources
                .FirstOrDefaultAsync(x => x.AssetIdentifier == file.AssetIdentifier);
            Assert.NotNull(resource);
            await RemoteFileManager.DeleteResource(resource);
        }

        foreach (var file in toBeDeleted)
        {
            await remoteValidator.ValidateFileDeleted(file);
        }
    }

    [Fact]
    public async Task TestReimportMedia()
    {
        var remoteValidator = new RemoteFileValidator(RemoteFileManager);
        var gameId = TestGameManager.GetGameId(Context, 3);

        var mediaBefore = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaBefore);
        Assert.NotNull(mediaBefore.Value);
        var countBefore = mediaBefore.Value.TotalCount;

        // now import several different media files to the game
        // skip the photo on the first import
        var files = new List<IFormFile>();
        var videoDirectory = Path.Join("data", "media", "video");
        var livePhotoDirectory = Path.Join("data", "media", "live photos");
        Dictionary<string, MediaResourceType> resourceTypes = [];
        foreach (var filePath in EnumerateMediaFiles(videoDirectory))
        {
            files.Add(CreateFormFile(filePath, out string fileName));
            resourceTypes[fileName] = MediaResourceType.Video;
        }
        foreach (var filePath in EnumerateMediaFiles(livePhotoDirectory))
        {
            files.Add(CreateFormFile(filePath, out string fileName));
            resourceTypes[fileName] = MediaResourceType.LivePhoto;
        }

        await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));

        var mediaAfter = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaAfter);
        Assert.NotNull(mediaAfter.Value);
        var countAfter = mediaAfter.Value.TotalCount;
        Assert.Equal(countBefore + 3, countAfter);

        var photoDirectory = Path.Join("data", "media", "photos");
        foreach (var filePath in EnumerateMediaFiles(photoDirectory))
        {
            files.Add(CreateFormFile(filePath, out string fileName));
            resourceTypes[fileName] = MediaResourceType.Photo;
        }
        await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));

        // Make sure we don't duplicate the video or live photos
        var mediaAfterReimport = await Controller.GetThumbnails(gameId: gameId);
        Assert.NotNull(mediaAfterReimport);
        Assert.NotNull(mediaAfterReimport.Value);
        var countAfterReimport = mediaAfterReimport.Value.TotalCount;
        Assert.Equal(countBefore + 4, countAfterReimport);

        List<RemoteFileDetail> toBeDeleted = [];
        foreach (var file in mediaAfter.Value.Results)
        {
            Assert.NotNull(file.Key);
            if (resourceTypes.TryGetValue(file.OriginalFileName, out MediaResourceType resourceType))
            {
                switch (resourceType)
                {
                    case MediaResourceType.Photo:
                        await ValidatePhoto(file.AssetIdentifier, file.OriginalFileName, toBeDeleted);
                        break;
                    case MediaResourceType.Video:
                        await ValidateVideo(file.AssetIdentifier, file.OriginalFileName, toBeDeleted);
                        break;
                    case MediaResourceType.LivePhoto:
                        await ValidateLivePhoto(file.AssetIdentifier, file.OriginalFileName, toBeDeleted);
                        break;
                    default:
                        Assert.Fail($"Unknown resource type {resourceType} for file {file.OriginalFileName}");
                        break;
                }
            }
            else
            {
                Assert.Fail($"No identified resource type for file {file.OriginalFileName}");
            }
        }

        foreach (var file in mediaAfter.Value.Results)
        {
            // now delete the resources and validate that the files are deleted from the remote
            var resource = await Context.MediaResources
                .FirstOrDefaultAsync(x => x.AssetIdentifier == file.AssetIdentifier);
            Assert.NotNull(resource);
            await RemoteFileManager.DeleteResource(resource);
        }

        foreach (var file in toBeDeleted)
        {
            await remoteValidator.ValidateFileDeleted(file);
        }
    }

    private async Task ImportMedia(List<IFormFile> files, long gameId, Dictionary<string, MediaResourceType> resourceTypes)
    {
        var importTask = await Controller.ImportMedia(files, JsonConvert.SerializeObject(gameId));
        Assert.NotNull(importTask);
        Assert.Equal(ImportTaskStatus.InProgress, importTask.Value.Status);
        Assert.Equal(0, importTask.Value.Progress);

        var photoCount = resourceTypes.Values.Count(r => r == MediaResourceType.Photo);
        var videoCount = resourceTypes.Values.Count(r => r == MediaResourceType.Video);
        var livePhotoCount = resourceTypes.Values.Count(r => r == MediaResourceType.LivePhoto);
        string expectedMessage = $"Importing {photoCount} photos, {videoCount} videos, and {livePhotoCount} live photos";
        Assert.Equal(expectedMessage, importTask.Value.Message);

        int i = 0;
        do
        {
            await Task.Delay(1000);
            importTask = await Controller.GetImportStatus(importTask.Value.Id);
            Assert.NotNull(importTask);
            Assert.Equal(ImportTaskStatus.InProgress, importTask.Value.Status);
            Assert.True(importTask.Value.Progress > 0 && importTask.Value.Progress < 1);
            Assert.Equal(expectedMessage, importTask.Value.Message);
            i++;
        } while (importTask.Value.Status == ImportTaskStatus.InProgress && i < 100);

        Assert.Equal(ImportTaskStatus.Completed, importTask.Value.Status);
        expectedMessage = $"Imported {photoCount} photos, {videoCount} videos, and {livePhotoCount} live photos";
        Assert.Equal(expectedMessage, importTask.Value.Message);
    }

    private IEnumerable<string> EnumerateMediaFiles(string directoryPath)
    {
        return Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories)
            .Where(file => !Path.GetExtension(file).Equals(".DS_Store", StringComparison.OrdinalIgnoreCase));
    }

    private FormFile CreateFormFile(string filePath, out string fileName)
    {
        fileName = Path.GetFileName(filePath);
        using var fileStream = File.OpenRead(filePath);
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
        {
            Headers = new HeaderDictionary(),
        };
        formFile.ContentType = GetContentType(fileName);
        return formFile;
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".heic" => "image/heic",
            ".mov" => "video/quicktime",
            ".mp4" => "video/mp4",
            _ => throw new NotSupportedException($"Unsupported file type: {extension}")
        };
    }

    private async Task ValidateLivePhoto(Guid assetIdentifier, string originalFileName, List<RemoteFileDetail> toBeDeleted)
    {
        var original = await Controller.GetOriginal(assetIdentifier);
        Assert.NotNull(original);
        Assert.NotNull(original.Value.Photo);
        Assert.Equal(".HEIC", original.Value.Photo.Value.Extension);
        Assert.NotNull(original.Value.Video);
        Assert.Equal(".mov", original.Value.Video.Value.Extension);
        Assert.NotNull(original.Value.AlternatePhoto);
        Assert.Equal(".jpeg", original.Value.AlternatePhoto.Value.Extension);
        Assert.NotNull(original.Value.AlternateVideo);
        Assert.Equal(".mp4", original.Value.AlternateVideo.Value.Extension);
        Assert.Equal(originalFileName, original.Value.Photo.Value.OriginalFileName);
        var expectedTime = ExpectedResourceTimes[originalFileName];
        Assert.Equal(expectedTime, original.Value.Photo.Value.DateTime);

        await RemoteValidator.ValidateFileExists(original.Value.Photo.Value);
        await RemoteValidator.ValidateFileExists(original.Value.Video.Value);
        await RemoteValidator.ValidateFileExists(original.Value.AlternatePhoto.Value);
        await RemoteValidator.ValidateFileExists(original.Value.AlternateVideo.Value);

        toBeDeleted.Add(original.Value.Photo.Value);
        toBeDeleted.Add(original.Value.Video.Value);
        toBeDeleted.Add(original.Value.AlternatePhoto.Value);
        toBeDeleted.Add(original.Value.AlternateVideo.Value);

        var thumbnails = await Context.MediaResources
            .Where(x => x.AssetIdentifier == assetIdentifier)
            .SelectMany(x => x.Files.Where(f => f.Purpose == RemoteFilePurpose.Thumbnail))
            .ToListAsync();

        Assert.Equal(3, thumbnails.Count);

        foreach (var thumbnail in thumbnails)
        {
            Assert.NotNull(thumbnail.NameModifier);
            var thumbnailDetail = await Controller.GetThumbnail(assetIdentifier, thumbnail.NameModifier);
            Assert.NotNull(thumbnailDetail);
            Assert.NotNull(thumbnailDetail.Value);
            await RemoteValidator.ValidateFileExists(thumbnailDetail.Value.Value);

            toBeDeleted.Add(thumbnailDetail.Value.Value);
        }
    }

    private async Task ValidatePhoto(Guid assetIdentifier, string originalFileName, List<RemoteFileDetail> toBeDeleted)
    {
        var original = await Controller.GetOriginal(assetIdentifier);
        Assert.NotNull(original);
        Assert.NotNull(original.Value.Photo);
        Assert.Equal(".HEIC", original.Value.Photo.Value.Extension);
        Assert.Null(original.Value.Video);
        Assert.NotNull(original.Value.AlternatePhoto);
        Assert.Equal(".jpeg", original.Value.AlternatePhoto.Value.Extension);
        Assert.Null(original.Value.AlternateVideo);
        Assert.Equal(originalFileName, original.Value.Photo.Value.OriginalFileName);
        var expectedTime = ExpectedResourceTimes[originalFileName];
        Assert.Equal(expectedTime, original.Value.Photo.Value.DateTime);

        await RemoteValidator.ValidateFileExists(original.Value.Photo.Value);
        await RemoteValidator.ValidateFileExists(original.Value.AlternatePhoto.Value);

        toBeDeleted.Add(original.Value.Photo.Value);
        toBeDeleted.Add(original.Value.AlternatePhoto.Value);

        var thumbnails = await Context.MediaResources
            .Where(x => x.AssetIdentifier == assetIdentifier)
            .SelectMany(x => x.Files.Where(f => f.Purpose == RemoteFilePurpose.Thumbnail))
            .ToListAsync();

        Assert.Equal(3, thumbnails.Count);

        foreach (var thumbnail in thumbnails)
        {
            Assert.NotNull(thumbnail.NameModifier);
            var thumbnailDetail = await Controller.GetThumbnail(assetIdentifier, thumbnail.NameModifier);
            Assert.NotNull(thumbnailDetail);
            Assert.NotNull(thumbnailDetail.Value);
            await RemoteValidator.ValidateFileExists(thumbnailDetail.Value.Value);

            toBeDeleted.Add(thumbnailDetail.Value.Value);
        }
    }

    private async Task ValidateVideo(Guid assetIdentifier, string originalFileName, List<RemoteFileDetail> toBeDeleted)
    {
        var original = await Controller.GetOriginal(assetIdentifier);
        Assert.NotNull(original);
        Assert.Null(original.Value.Photo);
        Assert.NotNull(original.Value.Video);
        Assert.Equal(".MOV", original.Value.Video.Value.Extension);
        Assert.Null(original.Value.AlternatePhoto);
        Assert.NotNull(original.Value.AlternateVideo);
        Assert.Equal(".mp4", original.Value.AlternateVideo.Value.Extension);
        Assert.Equal(originalFileName, original.Value.Video.Value.OriginalFileName);
        var expectedTime = ExpectedResourceTimes[originalFileName];
        Assert.Equal(expectedTime, original.Value.Video.Value.DateTime);

        await RemoteValidator.ValidateFileExists(original.Value.Video.Value);
        await RemoteValidator.ValidateFileExists(original.Value.AlternateVideo.Value);

        toBeDeleted.Add(original.Value.Video.Value);
        toBeDeleted.Add(original.Value.AlternateVideo.Value);

        var thumbnails = await Context.MediaResources
            .Where(x => x.AssetIdentifier == assetIdentifier)
            .SelectMany(x => x.Files.Where(f => f.Purpose == RemoteFilePurpose.Thumbnail))
            .ToListAsync();

        Assert.Equal(3, thumbnails.Count);

        foreach (var thumbnail in thumbnails)
        {
            Assert.NotNull(thumbnail.NameModifier);
            var thumbnailDetail = await Controller.GetThumbnail(assetIdentifier, thumbnail.NameModifier);
            Assert.NotNull(thumbnailDetail);
            Assert.NotNull(thumbnailDetail.Value);
            await RemoteValidator.ValidateFileExists(thumbnailDetail.Value.Value);

            toBeDeleted.Add(thumbnailDetail.Value.Value);
        }
    }

    internal static Dictionary<string, DateTimeOffset> ExpectedResourceTimes = new()
    {
        {"IMG_4721.HEIC", new DateTimeOffset(2024, 9, 28, 12, 43, 11, TimeSpan.FromHours(-5))},
        {"IMG_4762.HEIC", new DateTimeOffset(2024, 9, 28, 14, 19, 30, TimeSpan.FromHours(-5))},
        {"IMG_4771.HEIC", new DateTimeOffset(2024, 9, 28, 14, 47, 12, TimeSpan.FromHours(-5))},
        {"hevc.MOV", new DateTimeOffset(2021, 7, 28, 16, 12, 52, TimeSpan.FromHours(-4))}
    };
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
