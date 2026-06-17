using System;

namespace BaseballApi.Services;

public interface IMediaImportQueue
{
    public ValueTask PushAsync(Guid importId);

    public ValueTask<Guid> PopAsync(CancellationToken cancellationToken);

    public bool ImportInProgress { get; }

    /// <summary>Number of imports currently waiting in the queue; surfaced as a gauge metric.</summary>
    public int Count { get; }

    public void MarkImportInProgress();
    public void MarkImportComplete();
}
