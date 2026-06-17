using System.Threading.Channels;
using BaseballApi.Observability;

namespace BaseballApi.Services;

public class MediaImportQueue : IMediaImportQueue
{
    private Channel<Guid> Queue { get; } = Channel.CreateUnbounded<Guid>();
    private ILogger<MediaImportQueue> Logger { get; }
    private bool _importInProgress;

    public MediaImportQueue(ILogger<MediaImportQueue> logger)
    {
        Logger = logger;
        // Early-warning signal that the importer is falling behind, read from the unbounded
        // channel on each metric collection. No need to keep the returned instrument: the
        // Meter retains it, and observable gauges are pull-based so we never touch it again.
        Telemetry.Meter.CreateObservableGauge(
            "media_import.queue.depth",
            () => Count,
            unit: "{import}",
            description: "Media imports waiting in the queue to be processed.");
    }

    public int Count => Queue.Reader.Count;

    public async ValueTask PushAsync(Guid importId)
    {
        await Queue.Writer.WriteAsync(importId);
        Logger.LogInformation("Pushed import ID {ImportId} to the queue.", importId);
    }

    public async ValueTask<Guid> PopAsync(CancellationToken cancellationToken = default)
    {
        var result = await Queue.Reader.ReadAsync(cancellationToken);
        Logger.LogInformation("Popped import ID {ImportId} from the queue.", result);
        return result;
    }

    public bool ImportInProgress => _importInProgress;

    public void MarkImportInProgress()
    {
        _importInProgress = true;
        Logger.LogInformation("Media import marked as in progress.");
    }
    public void MarkImportComplete()
    {
        _importInProgress = false;
        Logger.LogInformation("Media import marked as complete.");
    }
}
