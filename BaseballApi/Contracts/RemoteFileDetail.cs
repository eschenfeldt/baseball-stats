using System;
using BaseballApi.Import;
using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct RemoteFileDetail(RemoteFile file)
{
    public Guid AssetIdentifier { get; set; } = file.Resource.AssetIdentifier;
    public DateTimeOffset DateTime { get; set; } = file.Resource.DateTime;
    public RemoteFilePurpose Purpose { get; set; } = file.Purpose;
    public string? NameModifier { get; set; } = file.NameModifier;
    public string Extension { get; set; } = file.Extension;
    public string OriginalFileName { get; set; } = file.Resource.OriginalFileName;
    public readonly string Key => $"{AssetIdentifier}/{this.Purpose.BaseFileName()}{this.NameModifier}{this.Extension}";
}
