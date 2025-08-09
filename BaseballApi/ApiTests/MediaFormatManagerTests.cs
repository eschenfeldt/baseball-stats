using System.Net;
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
    [InlineData("other/h264.MOV", false)]
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
    [InlineData("other/h264.MOV", false)]
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
    [InlineData("video/hevc.mov", true)]
    [InlineData("other/h264.MOV", false)] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    public async void TestContentTypeCorrected(string fileToUpload, bool generatesAltFormat)
    {
        // Upload a file then manipulate the content type in the bucket and make sure it gets corrected
        string fileName = await UploadFile(fileToUpload);
        var resource = await Context.MediaResources.Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.OriginalFileName == fileName);
        Assert.NotNull(resource);
        Assert.Equal(FileCount(generatesAltFormat), resource.Files.Count);
        var incorrectContentType = "binary/octet-stream";
        var toUpdate = resource.Files.Where(f => f.Purpose == RemoteFilePurpose.Original).ToList();
        Dictionary<long, string> expectedContentTypes = [];
        foreach (var file in toUpdate)
        {
            Assert.NotNull(file.ContentType);
            expectedContentTypes[file.Id] = file.ContentType;
            file.ContentType = incorrectContentType;
            var result = await RemoteFileManager.UpdateFileContentType(new RemoteFileDetail(file), incorrectContentType);
            Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);
            var metadata = await RemoteFileManager.GetFileMetadata(new RemoteFileDetail(file));
            Assert.Equal(incorrectContentType, metadata.Headers.ContentType);
        }
        await Context.SaveChangesAsync();

        var contentTypeResults = await Manager.SetContentTypes(fileName);
        Assert.Null(contentTypeResults.ErrorMessage);
        Assert.Equal(0, contentTypeResults.SetCount);
        Assert.Equal(toUpdate.Count, contentTypeResults.UpdateCount);

        foreach (var file in toUpdate)
        {
            Context.Entry(file).Reload();
            Assert.NotNull(file.ContentType);
            Assert.Equal(expectedContentTypes[file.Id], file.ContentType);
            var metadata = await RemoteFileManager.GetFileMetadata(new RemoteFileDetail(file));
            Assert.Equal(expectedContentTypes[file.Id], metadata.Headers.ContentType);
        }
    }

    [Theory]
    [InlineData("video/hevc.mov", true)]
    [InlineData("photos/IMG_4721.HEIC", true)]
    [InlineData("other/h264.MOV", false)]
    [InlineData("other/IMG_1278.JPG", false)]
    public async void TestAlternateFormatCreated(string fileToUpload, bool generatesAltFormat)
    {
        // Upload a file then delete the alternate formats from the db and bucket, then make sure they get recreated
        string fileName = await UploadFile(fileToUpload);

        async Task<MediaResource> LoadResource()
        {
            var resource = await Context.MediaResources
                .Include(r => r.Files)
                .FirstOrDefaultAsync(r => r.OriginalFileName == fileName);
            Assert.NotNull(resource);
            return resource;
        }

        var resource = await LoadResource();
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
        resource = await LoadResource();
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
        List<IFormFile> files = [];
        Dictionary<string, MediaResourceType> resourceTypes = [];
        string baseFileName = "IMG_4762";
        string newBaseFileName = baseFileName + Guid.NewGuid();
        var fileName = $"{newBaseFileName}.HEIC";
        var newPhotoPath = Path.Combine(Path.GetTempPath(), fileName);
        var newVideoPath = Path.Combine(Path.GetTempPath(), $"{newBaseFileName}.mov");
        File.Copy(Path.Combine("data", "media", "live photos", $"{baseFileName}.HEIC"), newPhotoPath);
        File.Copy(Path.Combine("data", "media", "live photos", $"{baseFileName}.mov"), newVideoPath);
        TestMediaImporter.PrepareIndividualFormFiles(MediaResourceType.LivePhoto, files, resourceTypes, newPhotoPath, newVideoPath);
        await Importer.ImportMedia(files, GameId, resourceTypes);
        File.Delete(newPhotoPath);
        File.Delete(newVideoPath);

        async Task<MediaResource> LoadResource()
        {
            var resource = await Context.MediaResources
                .Include(r => r.Files)
                .FirstOrDefaultAsync(r => r.OriginalFileName == fileName);
            Assert.NotNull(resource);
            return resource;
        }

        // Validate that the live photo was imported correctly
        var resource = await LoadResource();
        Assert.Equal(7, resource.Files.Count);

        // First delete the photo alternate format
        var photoFile = resource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.Extension == ".jpeg");
        Assert.NotNull(photoFile);
        resource.Files.Remove(photoFile);
        await RemoteFileManager.DeleteFile(new RemoteFileDetail(photoFile));
        await Context.SaveChangesAsync();

        var altFormatResults = await Manager.CreateAlternateFormats(fileName);
        Assert.Null(altFormatResults.ErrorMessage);
        Assert.Equal(1, altFormatResults.Count);
        resource = await LoadResource();
        var newPhotoFile = resource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.Extension == ".jpeg");
        Assert.NotNull(newPhotoFile);
        Assert.NotEqual(photoFile.Id, newPhotoFile.Id);
        await RemoteValidator.ValidateFileExists(new RemoteFileDetail(newPhotoFile), "image/jpeg");

        // Now delete the video alternate format
        var videoFile = resource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.Extension == ".mp4");
        Assert.NotNull(videoFile);
        resource.Files.Remove(videoFile);
        await RemoteFileManager.DeleteFile(new RemoteFileDetail(videoFile));
        await Context.SaveChangesAsync();

        altFormatResults = await Manager.CreateAlternateFormats(fileName);
        Assert.Null(altFormatResults.ErrorMessage);
        Assert.Equal(1, altFormatResults.Count);
        resource = await LoadResource();
        var newVideoFile = resource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.Extension == ".mp4");
        Assert.NotNull(newVideoFile);
        Assert.NotEqual(videoFile.Id, newVideoFile.Id);
        await RemoteValidator.ValidateFileExists(new RemoteFileDetail(newVideoFile), "video/mp4");

        // Finally delete both alternate formats and make sure they get recreated
        resource.Files.Remove(newPhotoFile);
        await RemoteFileManager.DeleteFile(new RemoteFileDetail(newPhotoFile));
        resource.Files.Remove(newVideoFile);
        await RemoteFileManager.DeleteFile(new RemoteFileDetail(newVideoFile));
        await Context.SaveChangesAsync();

        altFormatResults = await Manager.CreateAlternateFormats(fileName);
        Assert.Null(altFormatResults.ErrorMessage);
        Assert.Equal(1, altFormatResults.Count);
        resource = await LoadResource();
        newPhotoFile = resource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.Extension == ".jpeg");
        Assert.NotNull(newPhotoFile);
        Assert.NotEqual(photoFile.Id, newPhotoFile.Id);
        await RemoteValidator.ValidateFileExists(new RemoteFileDetail(newPhotoFile), "image/jpeg");
        newVideoFile = resource.Files.FirstOrDefault(f => f.Purpose == RemoteFilePurpose.AlternateFormat && f.Extension == ".mp4");
        Assert.NotNull(newVideoFile);
        Assert.NotEqual(videoFile.Id, newVideoFile.Id);
        await RemoteValidator.ValidateFileExists(new RemoteFileDetail(newVideoFile), "video/mp4");
    }

    [Fact]
    public async void TestAlternateFormatOverrideSet()
    {
        // Upload an h264 MOV file, then unset the alternate format override and make sure it gets reset
        // This is to simulate pre-existing files that don't have the override set
        string fileName = await UploadFile("other/h264.MOV");
        async Task<MediaResource> LoadResource()
        {
            var resource = await Context.MediaResources
                .Include(r => r.Files)
                .FirstOrDefaultAsync(r => r.OriginalFileName == fileName);
            Assert.NotNull(resource);
            return resource;
        }

        var resource = await LoadResource();
        Assert.Equal(FileCount(false), resource.Files.Count);
        Assert.True(resource.AlternateFormatOverride);

        resource.AlternateFormatOverride = false;
        await Context.SaveChangesAsync();

        var altFormatResults = await Manager.CreateAlternateFormats(fileName);
        Assert.Null(altFormatResults.ErrorMessage);
        // This will count as processing the file but won't actually create a new file
        Assert.Equal(1, altFormatResults.Count);
        resource = await LoadResource();
        Context.Entry(resource).Reload();
        Assert.Equal(FileCount(false), resource.Files.Count);
        Assert.True(resource.AlternateFormatOverride);
    }

    private async Task<string> UploadFile(string fileToUpload)
    {
        List<IFormFile> files = [];
        Dictionary<string, MediaResourceType> resourceTypes = [];
        // rename the file to avoid conflicts between tests
        string fileName = Path.GetFileNameWithoutExtension(fileToUpload);
        string newFileName = $"{fileName}{Guid.NewGuid()}{Path.GetExtension(fileToUpload)}";
        string path = Path.Combine(Path.GetTempPath(), newFileName);
        File.Copy(Path.Combine("data", "media", fileToUpload), path);
        TestMediaImporter.PrepareIndividualFormFiles(GetSingleFileResourceType(path), files, resourceTypes, path);
        await Importer.ImportMedia(files, GameId, resourceTypes);
        File.Delete(path);
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
