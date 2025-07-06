using System;

namespace BaseballApi.Models;

public class MediaImportTask
{
    public Guid Id { get; set; }
    public MediaImportTaskStatus Status { get; set; }
    public Game? Game { get; set; }
    public ICollection<MediaImportInfo> MediaToProcess { get; set; } = [];
}
