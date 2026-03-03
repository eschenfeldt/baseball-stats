using BaseballApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BaseballApiTests;

public sealed class FileLoggerTests : IDisposable
{
    string LogDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), nameof(FileLoggerTests), Guid.NewGuid().ToString("N"));
    LogTester LogTester { get; }
    LogTester2 LogTester2 { get; }
    RemoteLogManager RemoteLogManager { get; }

    public FileLoggerTests()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddProvider(new FileLoggerProvider(LogDirectory));
        });

        LogTester = new LogTester(loggerFactory.CreateLogger<LogTester>());
        LogTester2 = new LogTester2(loggerFactory.CreateLogger<LogTester2>());

        var builder = new ConfigurationBuilder()
            .AddJsonFile("/run/secrets/app_settings", optional: true)
            .AddUserSecrets<TestDatabaseFixture>();
        IConfiguration configuration = builder.Build();
        configuration.GetSection("Logging:File")["Directory"] = LogDirectory;
        // use a console logger for the remote log manager so we don't log to the files we're uploading
        var logManagerLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<RemoteLogManager>();
        RemoteLogManager = new RemoteLogManager(logManagerLogger, configuration, nameof(FileLoggerTests));
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

    [Fact]
    public void MultipleLogsAreWrittenThreadSafely()
    {
        Parallel.For(0, 100, i =>
        {
            LogTester.TestLog($"Log message {i}");
        });

        var logFiles = Directory.GetFiles(LogDirectory, "*.log");
        Assert.Single(logFiles);

        var logContents = File.ReadAllText(logFiles[0]);
        for (int i = 0; i < 100; i++)
        {
            Assert.Contains($"Log Tester 1 Log message {i}", logContents);
        }
    }

    [Fact]
    public void ExceptionsAreLogged()
    {
        var uniqueContent = Guid.NewGuid().ToString();
        try
        {
            throw new InvalidOperationException(uniqueContent);
        }
        catch (Exception ex)
        {
            LogTester.TestLogException(ex);
        }

        var logFiles = Directory.GetFiles(LogDirectory, "*.log");
        Assert.Single(logFiles);

        var logContents = File.ReadAllText(logFiles[0]);
        Assert.Contains(uniqueContent, logContents);
        Assert.Contains("An exception occurred", logContents);
        Assert.Contains("InvalidOperationException", logContents);
    }

    [Fact]
    public async Task LogsCanBeUploaded()
    {
        LogTester.TestLog("Upload test log message");

        var logFiles = Directory.GetFiles(LogDirectory, "*.log");
        Assert.Single(logFiles);

        var uploadedFilesBefore = await RemoteLogManager.GetUploadedLogFiles();
        Assert.Empty(uploadedFilesBefore);

        await RemoteLogManager.UploadPendingLogs(CancellationToken.None, allowInProgress: true);

        var uploadedFilesAfter = await RemoteLogManager.GetUploadedLogFiles();
        Assert.Single(uploadedFilesAfter);

        var uploadedFileKey = uploadedFilesAfter[0];
        Assert.Contains("FileLoggerTests", uploadedFileKey);
        Assert.Contains(DateTime.UtcNow.ToString("yyyy_MM_dd"), uploadedFileKey);
        Assert.EndsWith(".log", uploadedFileKey);

        // verify the local log file has been deleted
        logFiles = Directory.GetFiles(LogDirectory, "*.log");
        Assert.Empty(logFiles);

        await RemoteLogManager.CleanupOldLogs(CancellationToken.None, retainDays: 0);

        var uploadedFilesFinal = await RemoteLogManager.GetUploadedLogFiles();
        Assert.Empty(uploadedFilesFinal);
    }

    [Fact]
    public async Task ActiveLogsNotUploadedByDefault()
    {
        LogTester.TestLog("Active log test message");

        var logFiles = Directory.GetFiles(LogDirectory, "*.log");
        Assert.Single(logFiles);

        var uploadedFilesBefore = await RemoteLogManager.GetUploadedLogFiles();
        Assert.Empty(uploadedFilesBefore);

        await RemoteLogManager.UploadPendingLogs(CancellationToken.None);

        var uploadedFilesAfter = await RemoteLogManager.GetUploadedLogFiles();
        Assert.Empty(uploadedFilesAfter);

        // verify the local log file still exists
        logFiles = Directory.GetFiles(LogDirectory, "*.log");
        Assert.Single(logFiles);
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

    public void TestLogException(Exception ex)
    {
        logger.LogError(ex, "An exception occurred in Log Tester 1");
    }
}

class LogTester2(ILogger<LogTester2> logger)
{
    public void TestLog(string message)
    {
        logger.LogInformation("Log Tester 2 {message}", message);
    }
}
