using System;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Models;

[Index(nameof(AssetIdentifier), IsUnique = true)]
public class RemoteResource
{
    public long Id { get; set; }
    public Guid AssetIdentifier { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public required string OriginalFileName { get; set; }
    public ICollection<RemoteFile> Files { get; } = [];

    public Game? Game { get; set; }
}
