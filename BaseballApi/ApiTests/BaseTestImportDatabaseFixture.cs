using BaseballApi.Import;
using BaseballApi.Models;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace BaseballApiTests;

public class BaseTestImportDatabaseFixture : IDisposable
{
    private string DbName { get; }
    protected IConfiguration Configuration { get; }

    public BaseTestImportDatabaseFixture(string dbBaseName)
    {
        var configPath = Path.Join("/", "run", "secrets", "app_settings");
        var builder = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: true)
            .AddUserSecrets<BaseTestImportDatabaseFixture>();
        Configuration = builder.Build();

        DbName = $"{dbBaseName}UnitTest{Guid.NewGuid()}";

        using var context = CreateContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public BaseballContext CreateContext()
    {
        var ownerConnectionString = Configuration["Baseball:OwnerConnectionString"];
        // separate database for each fixture
        ownerConnectionString = ownerConnectionString?.Replace(
            "BaseballUnitTest",
            DbName
        );
        var options = new DbContextOptionsBuilder<BaseballContext>()
                                    .UseNpgsql(ownerConnectionString)
                                    .Options;
        return new BaseballContext(options);
    }

    public static GameMetadata PrepareGameForImport(out List<IFormFile> files)
    {
        files = [];
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
        return new GameMetadata
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
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }
}
