using System.Reflection;
using BaseballApi.Models;
using BaseballApi.Services;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public abstract class BaseballTests : IClassFixture<TestDatabaseFixture>, IDisposable
{
    protected BaseballContext Context { get; }
    protected TestDatabaseFixture Fixture { get; }
    protected ILoggerFactory FileLoggerFactory { get; }
    protected BaseballTests(TestDatabaseFixture fixture)
    {
        var projectRoot = ProjectRoot();
        Fixture = fixture;
        FileLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new FileLoggerProvider(Path.Combine(projectRoot, "Logs")));

            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
        });
        Context = TestDatabaseFixture.CreateContext();
        // Context.Database.BeginTransaction(); // allow changes without persisting to the db
    }

    private static string ProjectRoot()
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        while (dir != null && !Directory.GetFiles(dir, "*.csproj").Any())
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        return dir ?? Directory.GetCurrentDirectory();
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}