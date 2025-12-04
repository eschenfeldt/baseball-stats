namespace BaseballApi.Services;

public class RemoteLogService(ILogger<RemoteLogService> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private ILogger<RemoteLogService> Logger { get; } = logger;
    private IRemoteLogManager RemoteLogManager { get; } = serviceProvider.GetRequiredService<IRemoteLogManager>();
    private CancellationToken CancellationToken { get; set; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Remote log service started.");

        CancellationToken = cancellationToken;
        var uploadTimer = new Timer(UploadLogs, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        var cleanupTimer = new Timer(CleanupOldLogs, null, TimeSpan.FromHours(1), TimeSpan.FromHours(24));

        while (!CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, CancellationToken);
        }

        uploadTimer.Dispose();
        cleanupTimer.Dispose();

        Logger.LogInformation("Remote log service stopped.");
    }

    private async void UploadLogs(object? stateInfo)
    {
        try
        {
            await RemoteLogManager.UploadPendingLogs(CancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading logs.");
        }
    }

    private async void CleanupOldLogs(object? stateInfo)
    {
        try
        {
            await RemoteLogManager.CleanupOldLogs(CancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cleaning up old logs.");
        }
    }
}
