using System.Reflection;
using BaseballApi.Integrations;
using BaseballApi.Models;
using BaseballApi.Services;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public abstract class BaseballTests : IClassFixture<TestDatabaseFixture>, IClassFixture<TestFileLoggerFixture>, IDisposable
{
    protected BaseballContext Context { get; }
    protected TestDatabaseFixture Fixture { get; }
    protected ILoggerFactory FileLoggerFactory { get; }
    protected BaseballTests(TestDatabaseFixture fixture, TestFileLoggerFixture fileLoggerFixture)
    {
        Fixture = fixture;
        FileLoggerFactory = fileLoggerFixture.FileLoggerFactory;
        Context = TestDatabaseFixture.CreateContext();
        // Context.Database.BeginTransaction(); // allow changes without persisting to the db
    }

    protected static HttpClient CreateMLBAMHttpClient() => new()
    {
        BaseAddress = new Uri("https://statsapi.mlb.com/api/v1/")
    };

    protected static HttpClient CreateFangraphsHttpClient() => new()
    {
        BaseAddress = new Uri("https://www.fangraphs.com/api/search/players/")
    };

    protected static FangraphsConnector CreateFangraphsConnector() =>
        new(CreateFangraphsHttpClient());

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}