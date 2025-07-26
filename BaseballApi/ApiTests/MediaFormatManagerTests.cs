using System;
using BaseballApi.Import;
using BaseballApi.Models;
using BaseballApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public class MediaFormatManagerTests : IClassFixture<TestImportDatabaseFixture>, IDisposable
{
    TestImportDatabaseFixture Fixture { get; }
    BaseballContext Context { get; }
    RemoteFileManager RemoteFileManager { get; }
    RemoteFileValidator RemoteValidator { get; }
    MediaFormatManager Manager { get; }

    public MediaFormatManagerTests(TestImportDatabaseFixture fixture)
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

        // Prepare the media import background service
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<IRemoteFileManager>(RemoteFileManager);
        services.AddDbContext<BaseballContext>(opt =>
            opt.UseNpgsql(Context.Database.GetConnectionString()));
        var serviceProvider = services.BuildServiceProvider();
        Manager = new(mediaImportQueue, serviceProvider, logger, CancellationToken.None);
    }

    [Theory]
    [InlineData("video/hevc.mov")]
    [InlineData("photos/IMG_4721.HEIC")]
    [InlineData("other/h264.MOV")] // This is probably going to flag incorrectly; it does work in Firefox as binary/octect-stream with .MOV extension
    [InlineData("other/IMG_1278.JPG")]
    public async void TestNoCleanupRequired(string fileToUpload)
    {
        // Upload a file through the regular endpoint and validate that it doesn't require background cleanup
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

    public void Dispose()
    {
        Fixture.Dispose();
    }
}
