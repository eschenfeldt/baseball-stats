using System;
using System.Diagnostics.CodeAnalysis;
using BaseballApi.Import;
using BaseballApi.Models;
using Humanizer;

namespace BaseballApi.Contracts;

public struct RemoteFileDetail
{
    public Guid AssetIdentifier { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public required string FileType { get; set; }
    public RemoteFilePurpose Purpose { get; set; }
    public string? NameModifier { get; set; }
    public required string Extension { get; set; }
    public required string OriginalFileName { get; set; }
    private readonly string NameModifierForKey => this.NameModifier == null ? "" : $"_{this.NameModifier}";
    public readonly string Key => $"{AssetIdentifier.ToString("D").ToUpper()}/{this.Purpose.BaseFileName()}{this.NameModifierForKey}{this.Extension}";

    public RemoteFileDetail() { }

    [SetsRequiredMembers]
    public RemoteFileDetail(RemoteFile file)
    {
        AssetIdentifier = file.Resource.AssetIdentifier;
        DateTime = file.Resource.DateTime;
        Purpose = file.Purpose;
        NameModifier = file.NameModifier;
        Extension = file.Extension;
        OriginalFileName = file.Resource.OriginalFileName;
        if (file.Resource is MediaResource media)
        {
            FileType = media.ResourceType.Humanize();
        }
        else if (file.Resource is Scorecard)
        {
            FileType = MediaResourceType.Scorecard.Humanize();
        }
        else
        {
            FileType = "Error";
        }
    }
}
