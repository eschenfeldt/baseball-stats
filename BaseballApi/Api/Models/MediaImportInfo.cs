namespace BaseballApi.Models;

public class MediaImportInfo
{
    public Guid Id { get; set; }
    public required string BaseName { get; set; }
    public MediaResourceType ResourceType { get; set; }
    public string? PhotoFilePath { get; set; }
    public string? PhotoFileName { get; set; }
    public string? VideoFilePath { get; set; }
    public string? VideoFileName { get; set; }
    public MediaImportTaskStatus Status { get; set; } = MediaImportTaskStatus.Queued;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
