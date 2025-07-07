using System;

namespace BaseballApi.Services;

public interface IMediaImportQueue
{
    public ValueTask PushAsync(Guid importId);

    public ValueTask<Guid> PopAsync(CancellationToken cancellationToken);
}
