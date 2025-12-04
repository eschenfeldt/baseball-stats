using System;

namespace BaseballApi.Services;

public interface IRemoteLogManager
{
    Task UploadPendingLogs(CancellationToken cancellationToken, bool allowInProgress = false);
    Task CleanupOldLogs(CancellationToken cancellationToken, int? retainDays = null);
    Task<List<string>> GetUploadedLogFiles();
}
