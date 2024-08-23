using System;
using BaseballApi.Models;

namespace BaseballApi.Import;

public static class RemoteFileExtensions
{
    public static string BaseFileName(this RemoteFilePurpose purpose)
    {
        return purpose switch
        {
            RemoteFilePurpose.Original => "original",
            RemoteFilePurpose.Thumbnail => "thumbnail",
            RemoteFilePurpose.AlternateFormat => "alt",
            _ => ""
        };
    }
}
