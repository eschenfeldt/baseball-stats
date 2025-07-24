using System;
using BaseballApi.Models;

namespace BaseballApi.Media;

public class MediaUploadResult
{
    public required MediaImportInfo OriginalResource { get; set; }
    public MediaResource? Resource { get; set; }
    public string? ErrorMessage { get; set; }
}
