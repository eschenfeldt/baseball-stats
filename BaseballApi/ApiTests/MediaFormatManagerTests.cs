using System;
using BaseballApi.Contracts;
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

public class MediaFormatManagerTests : IClassFixture<TestMediaImportDatabaseFixture>
{
    BaseballContext Context { get; }
    TestMediaImportDatabaseFixture Fixture { get; }
    RemoteFileManager RemoteFileManager { get; }
    RemoteFileValidator RemoteValidator { get; }
    MediaFormatManager Manager { get; }
    TestMediaImporter Importer { get; }
    long GameId { get { return Fixture.GameId; } }

    public MediaFormatManagerTests(TestMediaImportDatabaseFixture fixture)
    {
        Fixture = fixture;
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

    [Theory]
    [InlineData("video/hevc.mov", true)]
    [InlineData("photos/IMG_4721.HEIC", true)]
    [InlineData("other/h264.MOV", false)] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    [InlineData("other/IMG_1278.JPG", false)]
    public async void TestNoCleanupRequired(string fileToUpload, bool generatesAltFormat)
    {
        // Upload a file through the regular endpoint and validate that it doesn't require background cleanup
        string fileName = await UploadFile(fileToUpload);

        var fileCount = await Context.MediaResources.Where(r => r.OriginalFileName == fileName).SelectMany(r => r.Files).CountAsync();
        Assert.Equal(FileCount(generatesAltFormat), fileCount);

        var contentTypeResults = await Manager.SetContentTypes(fileName);
        Assert.Null(contentTypeResults.ErrorMessage);
        Assert.Equal(0, contentTypeResults.SetCount);
        Assert.Equal(0, contentTypeResults.UpdateCount);

        var altFormatResults = await Manager.CreateAlternateFormats(fileName);
        Assert.Null(contentTypeResults.ErrorMessage);
        Assert.Equal(0, altFormatResults.Count);
    }

    [Theory]
    [InlineData("video/hevc.mov", true)]
    [InlineData("photos/IMG_4721.HEIC", true)]
    [InlineData("other/h264.MOV", false)] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    [InlineData("other/IMG_1278.JPG", false)]
    public async void TestContentTypeSet(string fileToUpload, bool generatesAltFormat)
    {
        // Upload a file then delete the content type in the db and make sure it gets set again
        string fileName = await UploadFile(fileToUpload);
        var resource = await Context.MediaResources.Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.OriginalFileName == fileName);
        Assert.NotNull(resource);
        Assert.Equal(FileCount(generatesAltFormat), resource.Files.Count);
        Dictionary<long, string> expectedContentTypes = [];
        foreach (var file in resource.Files)
        {
            Assert.NotNull(file.ContentType);
            expectedContentTypes[file.Id] = file.ContentType;
            file.ContentType = null;
        }
        await Context.SaveChangesAsync();
        var fileCountWithContentType = await Context.MediaResources.Where(r => r.OriginalFileName == fileName)
            .SelectMany(r => r.Files)
            .Where(f => f.ContentType != null)
            .CountAsync();
        Assert.Equal(0, fileCountWithContentType);
        var contentTypeResults = await Manager.SetContentTypes(fileName);
        Assert.Null(contentTypeResults.ErrorMessage);
        Assert.Equal(FileCount(generatesAltFormat), contentTypeResults.SetCount);
        Assert.Equal(0, contentTypeResults.UpdateCount);

        foreach (var file in resource.Files)
        {
            Context.Entry(file).Reload();
            Assert.NotNull(file.ContentType);
            Assert.Equal(expectedContentTypes[file.Id], file.ContentType);
        }
    }


    [Theory]
    [InlineData("video/hevc.mov")]
    [InlineData("other/h264.MOV")] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    public async void TestContentTypeCorrected(string fileToUpload)
    {
        // Upload a file then manipulate the content type in the bucket and make sure it gets corrected
    }

    [Theory]
    [InlineData("video/hevc.mov", true)]
    [InlineData("photos/IMG_4721.HEIC", true)]
    [InlineData("other/h264.MOV", false)] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    [InlineData("other/IMG_1278.JPG", false)]
    public async void TestAlternateFormatCreated(string fileToUpload, bool generatesAltFormat)
    {
        // Upload a file then delete the alternate formats from the db and bucket, then make sure they get recreated
        string fileName = await UploadFile(fileToUpload);
        var resource = await Context.MediaResources
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.OriginalFileName == fileName);
        Assert.NotNull(resource);
        Assert.Equal(FileCount(generatesAltFormat), resource.Files.Count);
        string? expectedContentType = null;
        var files = resource.Files.ToList();
        foreach (var file in files)
        {
            if (file.Purpose == RemoteFilePurpose.AlternateFormat)
            {
                Assert.NotNull(file.ContentType);
                expectedContentType = file.ContentType;
                resource.Files.Remove(file);
                await RemoteFileManager.DeleteFile(new RemoteFileDetail(file));
            }
        }
        await Context.SaveChangesAsync();
        var altFormatResults = await Manager.CreateAlternateFormats(fileName);
        Assert.Equal(generatesAltFormat ? 1 : 0, altFormatResults.Count);
        Assert.Null(altFormatResults.ErrorMessage);
        if (generatesAltFormat && expectedContentType != null)
        {
            var newAltFormat = resource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat);
            Assert.NotNull(newAltFormat);
            Assert.NotEqual(0, newAltFormat.Id);
            await RemoteValidator.ValidateFileExists(new RemoteFileDetail(newAltFormat), expectedContentType);
        }
        else if (generatesAltFormat)
        {
            Assert.Fail("Expected content type not found");
        }
        else
        {
            Assert.Empty(resource.Files.Where(f => f.Purpose == RemoteFilePurpose.AlternateFormat));
        }
    }

    [Fact]
    public async void TestAlternateFormatLivePhoto()
    {
        // upload a live photo, delete one alternate file, check it recreates, then repeat for the other file and both files at once
    }

    private async Task<string> UploadFile(string fileToUpload)
    {
        List<IFormFile> files = [];
        Dictionary<string, MediaResourceType> resourceTypes = [];
        // rename the file to avoid conflicts between tests
        string fileName = Path.GetFileNameWithoutExtension(fileToUpload);
        string newFileName = $"{fileName}{Guid.NewGuid()}{Path.GetExtension(fileToUpload)}";
        string path = Path.Combine("data", "media", newFileName);
        File.Copy(Path.Combine("data", "media", fileToUpload), path, true);
        TestMediaImporter.PrepareIndividualFormFiles(GetSingleFileResourceType(path), files, resourceTypes, path);
        await Importer.ImportMedia(files, GameId, resourceTypes);
        return newFileName;
    }

    private static int FileCount(bool generatesAltFormat)
    {
        return generatesAltFormat ? 5 : 4;
    }

    private static MediaResourceType GetSingleFileResourceType(string path)
    {
        string extension = new FileInfo(path).Extension.ToLowerInvariant();
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
            case ".heic":
                return MediaResourceType.Photo;
            case ".mov":
            case ".mp4":
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
}
