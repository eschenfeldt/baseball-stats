namespace BaseballApi.Services;

public class FileLogger(string directory, string category, object sharedLock) : ILogger
{
    private string LogDirectory { get; } = directory;
    private string Category { get; } = category;
    private object Lock { get; } = sharedLock;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        private NullScope() { }
        public void Dispose() { }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var filename = Path.Combine(LogDirectory, $"{DateTime.UtcNow:yyyy_MM_dd}.log");
        var message = $"{DateTime.UtcNow:o} [{logLevel}] {Category}: {formatter(state, exception)}{Environment.NewLine}";

        lock (Lock)
        {
            File.AppendAllText(filename, message);
        }
    }
}

