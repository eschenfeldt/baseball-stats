using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct ImportTask()
{
    public Guid Id { get; set; }
    public MediaImportTaskStatus Status { get; set; }
    public decimal Progress { get; set; }
    public required string Message { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}
