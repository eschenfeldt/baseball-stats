using BaseballApi.Services;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public sealed class FileLoggerTests : IDisposable
{
    string LogDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), nameof(FileLoggerTests), Guid.NewGuid().ToString("N"));
    LogTester LogTester { get; }
    LogTester2 LogTester2 { get; }

    public FileLoggerTests()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new FileLoggerProvider(LogDirectory));
        });

        LogTester = new LogTester(loggerFactory.CreateLogger<LogTester>());
        LogTester2 = new LogTester2(loggerFactory.CreateLogger<LogTester2>());
    }

    [Fact]
    public void LogsAreWritten()
    {
        LogTester.TestLog("First log message");
        LogTester2.TestLog("Second log message");

        var logFiles = Directory.GetFiles(LogDirectory, "*.log");
        Assert.Single(logFiles);

        var logContents = File.ReadAllText(logFiles[0]);
        Assert.Contains("Log Tester 1 First log message", logContents);
        Assert.Contains("Log Tester 2 Second log message", logContents);
    }

    public void Dispose()
    {
        if (Directory.Exists(LogDirectory))
        {
            Directory.Delete(LogDirectory, true);
        }
    }
}

class LogTester(ILogger<LogTester> logger)
{
    public void TestLog(string message)
    {
        logger.LogInformation("Log Tester 1 {message}", message);
    }
}

class LogTester2(ILogger<LogTester2> logger)
{
    public void TestLog(string message)
    {
        logger.LogInformation("Log Tester 2 {message}", message);
    }
}
