using System;

namespace BaseballApi.Services;

public interface IMediaImportQueue
{
    public ValueTask PushAsync(Guid importId);

    public ValueTask<Guid> PopAsync(CancellationToken cancellationToken);

    public bool ImportInProgress { get; }
    public void MarkImportInProgress();
    public void MarkImportComplete();
}
