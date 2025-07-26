using System;
using BaseballApi.Controllers;
using BaseballApi.Import;
using BaseballApi.Models;
using BaseballApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BaseballApiTests;

public class MediaFormatManagerTests : BaseImportTests, IAsyncLifetime
{
    BaseballContext Context { get; }
    RemoteFileManager RemoteFileManager { get; }
    RemoteFileValidator RemoteValidator { get; }
    MediaFormatManager Manager { get; }
    TestMediaImporter Importer { get; }
    long GameId { get; set; }

    public MediaFormatManagerTests(TestImportDatabaseFixture fixture) : base(fixture)
    {
        Context = Fixture.CreateContext();

        var builder = new ConfigurationBuilder()
            .AddJsonFile("/run/secrets/app_settings", optional: true)
            .AddUserSecrets<TestImportDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        RemoteFileManager = new(configuration, nameof(MediaFormatManagerTests));
        RemoteValidator = new(RemoteFileManager);
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<MediaImportQueue>();
        MediaImportQueue mediaImportQueue = new(logger);

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IRemoteFileManager>(RemoteFileManager);
        services.AddDbContext<BaseballContext>(opt =>
            opt.UseNpgsql(Context.Database.GetConnectionString()));
        var serviceProvider = services.BuildServiceProvider();
        var backgroundLogger = loggerFactory.CreateLogger<MediaImportBackgroundService>();
        var backgroundService = new MediaImportBackgroundService(mediaImportQueue, serviceProvider, backgroundLogger);
        Manager = new(mediaImportQueue, serviceProvider, logger, CancellationToken.None);

        var controller = new MediaController(Context, RemoteFileManager, mediaImportQueue);
        Importer = new(Context, controller, backgroundService);
    }

    public async Task InitializeAsync()
    {
        // import a game; files will attach to this game and 
        // it will include a scorecard to be sure that doesn't get flagged by any of these processes
        var gamesController = new GamesController(Context, RemoteFileManager);
        var metadata = PrepareGameForImport(out List<IFormFile> files);
        var result = await gamesController.ImportGame(files, JsonConvert.SerializeObject(metadata));
        Assert.NotNull(result);
        GameId = result.Value.Id;
    }


    [Theory]
    [InlineData("video/hevc.mov")]
    [InlineData("photos/IMG_4721.HEIC")]
    [InlineData("other/h264.MOV")] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    [InlineData("other/IMG_1278.JPG")]
    public async void TestNoCleanupRequired(string fileToUpload)
    {
        // Upload a file through the regular endpoint and validate that it doesn't require background cleanup
        List<IFormFile> files = [];
        Dictionary<string, MediaResourceType> resourceTypes = [];
        string path = Path.Combine("data", "media", fileToUpload);
        TestMediaImporter.PrepareIndividualFormFiles(GetSingleFileResourceType(path), files, resourceTypes, path);
        await Importer.ImportMedia(files, GameId, resourceTypes);

        var fileCount = await Context.MediaResources.SelectMany(r => r.Files).CountAsync();
        Assert.True(fileCount > 1, "Scorecard and file uploaded during this test should be registered in db");

        var contentTypeResults = await Manager.SetContentTypes();
        Assert.Equal(0, contentTypeResults.SetCount);
        Assert.Equal(0, contentTypeResults.UpdateCount);
        Assert.Null(contentTypeResults.ErrorMessage);

        var altFormatResults = await Manager.CreateAlternateFormats();
        Assert.Equal(0, altFormatResults.Count);
        Assert.Null(contentTypeResults.ErrorMessage);
    }

    [Theory]
    [InlineData("video/hevc.mov")]
    [InlineData("photos/IMG_4721.HEIC")]
    [InlineData("other/h264.MOV")] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    [InlineData("other/IMG_1278.JPG")]
    public async void TestContentTypeSet(string fileToUpload)
    {
        // Upload a file then delete the content type in the db and make sure it gets set again
    }


    [Theory]
    [InlineData("video/hevc.mov")]
    [InlineData("other/h264.MOV")] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    public async void TestContentTypeCorrected(string fileToUpload)
    {
        // Upload a file then manipulate the content type in the bucket and make sure it gets corrected
    }

    [Theory]
    [InlineData("video/hevc.mov")]
    [InlineData("photos/IMG_4721.HEIC")]
    [InlineData("other/h264.MOV")] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    [InlineData("other/IMG_1278.JPG")]
    public async void TestAlternateFormatCreated(string fileToUpload)
    {
        // Upload a file then delete the alternate format from the db and bucket, then make sure it gets recreated
    }

    [Fact]
    public async void TestAlternateFormatLivePhoto()
    {
        // upload a live photo, delete one alternate file, check it recreates, then repeat for the other file and both files at once
    }

    private static MediaResourceType GetSingleFileResourceType(string path)
    {
        string extension = new FileInfo(path).Extension.ToLowerInvariant();
        switch (extension)
        {
            case "jpg":
            case "jpeg":
            case "heic":
                return MediaResourceType.Photo;
            case "mov":
            case "mp4":
                return MediaResourceType.Video;
            default:
                throw new ArgumentException($"Unexpected extension {extension}");
        }
    }

    public Task DisposeAsync()
    {
        // TODO: Go through database and delete every remote resource from the bucket
        // don't worry about db cleanup, though
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Fixture.Dispose();
    }

}
