using System.Threading.Channels;
using BaseballApi.Observability;

namespace BaseballApi.Services;

public class MediaImportQueue : IMediaImportQueue
{
    private Channel<Guid> Queue { get; } = Channel.CreateUnbounded<Guid>();
    private ILogger<MediaImportQueue> Logger { get; }
    private bool _importInProgress;

    public MediaImportQueue(ILogger<MediaImportQueue> logger, MediaImportMetrics metrics)
    {
        Logger = logger;
        // The queue-depth gauge is defined on MediaImportMetrics; bind it to this queue's count so
        // the importer's backlog shows up as an early-warning signal when it's falling behind.
        metrics.TrackQueueDepth(() => Count);
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
