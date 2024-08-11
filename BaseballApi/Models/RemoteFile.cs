using System;

namespace BaseballApi.Models;

public class RemoteFile
{
    public long Id { get; set; }

    public required RemoteResource Resource { get; set; }

    public RemoteFilePurpose Purpose { get; set; }
    public string? NameModifier { get; set; }
    public required string Extension { get; set; }
}
