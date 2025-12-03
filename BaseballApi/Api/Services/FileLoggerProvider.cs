namespace BaseballApi.Services;

public class FileLoggerProvider : ILoggerProvider
{
    private string LogDirectory { get; }
    private readonly object _lock = new();

    public FileLoggerProvider(string directory)
    {
        LogDirectory = directory;
        Directory.CreateDirectory(directory);
    }

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(LogDirectory, categoryName, _lock);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
