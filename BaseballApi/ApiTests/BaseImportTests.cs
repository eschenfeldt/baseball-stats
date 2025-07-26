using System;
using BaseballApi.Controllers;
using BaseballApi.Import;
using BaseballApi.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BaseballApiTests;

public class BaseImportTests(TestImportDatabaseFixture fixture) : IClassFixture<TestImportDatabaseFixture>, IDisposable
{
    protected TestImportDatabaseFixture Fixture { get; } = fixture;

    protected static GameMetadata PrepareGameForImport(out List<IFormFile> files)
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
        Fixture.Dispose();
    }
}
