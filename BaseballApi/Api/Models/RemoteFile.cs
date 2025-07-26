using System;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Models;

[Index(nameof(ResourceId))]
[Index(nameof(ResourceId), nameof(NameModifier), nameof(Extension), IsUnique = true)]
public class RemoteFile
{
    public long Id { get; set; }

    public required RemoteResource Resource { get; set; }
    public long ResourceId { get; set; }

    public RemoteFilePurpose Purpose { get; set; }
    public string? NameModifier { get; set; }
    public required string Extension { get; set; }
    public string? ContentType { get; set; }
}
