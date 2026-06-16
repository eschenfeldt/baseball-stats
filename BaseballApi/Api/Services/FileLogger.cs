using System.Diagnostics;

namespace BaseballApi.Services;

public class FileLogger(string directory, string category, object sharedLock, Func<IExternalScopeProvider?> scopeProvider) : ILogger
{
    private string LogDirectory { get; } = directory;
    private string Category { get; } = category;
    private object Lock { get; } = sharedLock;
    private Func<IExternalScopeProvider?> ScopeProvider { get; } = scopeProvider;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return ScopeProvider()?.Push(state) ?? NullScope.Instance;
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

        // Match the trace id exported via OpenTelemetry so file lines can be
        // cross-referenced with traces
        var traceId = Activity.Current is { } activity ? $" [{activity.TraceId}]" : "";

        // Render active scopes (e.g. ImportId) so file lines keep parity with the
        // attributes exported to OTLP
        var scopeText = FormatScopes();

        lock (Lock)
        {
            File.AppendAllText(filename, $"{timestamp} [{logLevel}] {Category}{traceId}{scopeText}: {message}{Environment.NewLine}");
        }
    }

    private string FormatScopes()
    {
        var provider = ScopeProvider();
        if (provider is null)
        {
            return "";
        }

        var sb = new System.Text.StringBuilder();
        provider.ForEachScope((scope, state) =>
        {
            if (scope is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                foreach (var pair in pairs)
                {
                    // Skip the structured-logging message template entry
                    if (pair.Key == "{OriginalFormat}")
                    {
                        continue;
                    }
                    state.Append($" {pair.Key}={pair.Value}");
                }
            }
            else if (scope is not null)
            {
                state.Append($" {scope}");
            }
        }, sb);
        return sb.ToString();
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

