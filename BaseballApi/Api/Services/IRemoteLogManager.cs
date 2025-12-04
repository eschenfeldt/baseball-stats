using System;

namespace BaseballApi.Services;

public interface IRemoteLogManager
{
    Task UploadPendingLogs(CancellationToken cancellationToken);
    Task CleanupOldLogs(CancellationToken cancellationToken);
}
