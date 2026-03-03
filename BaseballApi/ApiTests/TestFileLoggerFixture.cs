using System;
using System.Reflection;
using BaseballApi.Services;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public class TestFileLoggerFixture : IDisposable
{
    public ILoggerFactory FileLoggerFactory { get; }

    public TestFileLoggerFixture()
    {
        var projectRoot = ProjectRoot();
        FileLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new FileLoggerProvider(Path.Combine(projectRoot, "Logs")));

            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
        });
    }

    private static string ProjectRoot()
    {
        string? dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        while (dir != null && Directory.GetFiles(dir, "*.csproj").Length == 0)
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        return dir ?? Directory.GetCurrentDirectory();
    }

    public void Dispose()
    {
        FileLoggerFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}
