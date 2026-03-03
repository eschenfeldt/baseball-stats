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
        ArgumentNullException.ThrowIfNull(formatter);

        var now = DateTime.UtcNow;
        var timestamp = now.ToString("o");
        var message = formatter(state, exception);

        // Append exception details if present
        if (exception != null)
        {
            message += Environment.NewLine + FormatException(exception);
        }
        var filename = Path.Combine(LogDirectory, $"{now:yyyy_MM_dd}.log");

        lock (Lock)
        {
            File.AppendAllText(filename, $"{timestamp} [{logLevel}] {Category}: {message}{Environment.NewLine}");
        }
    }

    private static string FormatException(Exception ex)
    {
        // Recursively format inner exceptions
        var sb = new System.Text.StringBuilder();
        int depth = 0;

        while (ex != null)
        {
            sb.AppendLine($"Exception Level {depth}: {ex.GetType().FullName}: {ex.Message}");
            sb.AppendLine(ex.StackTrace ?? "(no stack trace)");
            if (ex.InnerException == null)
            {
                break;
            }
            else
            {
                ex = ex.InnerException;
                depth++;
            }
        }

        return sb.ToString();
    }
}

