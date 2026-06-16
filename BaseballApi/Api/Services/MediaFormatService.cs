using BaseballApi.Observability;

namespace BaseballApi.Services;

public class MediaFormatService(
        IMediaImportQueue mediaImportQueue,
        IServiceProvider serviceProvider,
        ILogger<MediaFormatService> logger) : BackgroundService
{
    private IMediaImportQueue MediaImportQueue { get; } = mediaImportQueue;
    private IServiceProvider ServiceProvider { get; } = serviceProvider;
    private ILogger<MediaFormatService> Logger { get; } = logger;
    private CancellationToken CancellationToken { get; set; }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Remote file format manager started.");
        var contentTypesTimer = new Timer(SetContentTypes, null, TimeSpan.Zero, TimeSpan.FromHours(12));
        var alternateFormatTimer = new Timer(CreateAlternateFormats, null, TimeSpan.FromMinutes(30), TimeSpan.FromHours(1));
        CancellationToken = cancellationToken;
        // Wait for the timers to trigger
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        contentTypesTimer.Dispose();
        alternateFormatTimer.Dispose();
        Logger.LogInformation("Remote file format manager stopped.");
    }

    private async void SetContentTypes(object? stateInfo)
    {
        using var activity = Telemetry.BackgroundJobs.StartActivity("job.set-content-types");
        try
        {
            var mediaFormatManager = new MediaFormatManager(MediaImportQueue, ServiceProvider, Logger, CancellationToken);
            await mediaFormatManager.SetContentTypes();
        }
        catch (Exception ex)
        {
            Telemetry.RecordJobException(activity, ex);
            Logger.LogError(ex, "Error setting content types.");
        }
    }

    private async void CreateAlternateFormats(object? stateInfo)
    {
        using var activity = Telemetry.BackgroundJobs.StartActivity("job.create-alternate-formats");
        try
        {
            var mediaFormatManager = new MediaFormatManager(MediaImportQueue, ServiceProvider, Logger, CancellationToken);
            await mediaFormatManager.CreateAlternateFormats();
        }
        catch (Exception ex)
        {
            Telemetry.RecordJobException(activity, ex);
            Logger.LogError(ex, "Error creating alternate formats.");
        }
    }
}
