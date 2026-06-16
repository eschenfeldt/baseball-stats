namespace BaseballApi.Services;

public class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private string LogDirectory { get; }
    private readonly object _lock = new();
    private IExternalScopeProvider? _scopeProvider;

    public FileLoggerProvider(string directory)
    {
        LogDirectory = directory;
        Directory.CreateDirectory(directory);
    }

    // Called by the logging factory after the provider is registered; may run
    // after loggers are created, so loggers read it lazily via the closure below.
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        => _scopeProvider = scopeProvider;

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(LogDirectory, categoryName, _lock, () => _scopeProvider);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
