using System;

namespace BaseballApi.Contracts;

public enum ImportTaskStatus
{
    InProgress,
    Completed,
    Failed
}

public struct ImportTask
{
    public Guid Id { get; set; }
    public ImportTaskStatus Status { get; set; }
    public decimal Progress { get; set; }
    public string Message { get; set; }
}
