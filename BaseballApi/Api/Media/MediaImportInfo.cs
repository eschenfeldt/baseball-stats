using System;
using BaseballApi.Models;

namespace BaseballApi.Media;

public class MediaImportInfo
{
    public required string BaseName { get; set; }
    public MediaResourceType ResourceType { get; set; }
    public string? PhotoFilePath { get; set; }
    public string? PhotoFileName { get; set; }
    public string? VideoFilePath { get; set; }
    public string? VideoFileName { get; set; }
}
