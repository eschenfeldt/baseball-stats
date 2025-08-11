using System.Threading.Channels;

namespace BaseballApi.Services;

public class MediaImportQueue(ILogger<MediaImportQueue> logger) : IMediaImportQueue
{
    private Channel<Guid> Queue { get; } = Channel.CreateUnbounded<Guid>();
    private ILogger<MediaImportQueue> Logger { get; } = logger;
    private bool _importInProgress;

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
