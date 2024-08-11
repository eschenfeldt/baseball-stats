using System;

namespace BaseballApi.Models;

public class RemoteResource
{
    public long Id { get; set; }
    public Guid AssetIdentifier { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public required string OriginalFileName { get; set; }
    public ICollection<RemoteFile> Files { get; } = [];

    public Game? Game { get; set; }
    public long? GameId { get; set; }
}
