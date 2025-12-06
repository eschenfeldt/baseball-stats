using System.Reflection;
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

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}