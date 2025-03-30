using System;
using BaseballApi.Models;

namespace BaseballApi.Media;

public class MediaImportInfo
{
    public required string BaseName { get; set; }
    public MediaResourceType ResourceType { get; set; }
    public Dictionary<string, string> FilePaths { get; } = [];
}
