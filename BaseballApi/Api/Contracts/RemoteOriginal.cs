using System;

namespace BaseballApi.Contracts;

public struct RemoteOriginal
{
    public required string FileType { get; set; }
    public string? GameName { get; set; }
    public DateOnly? GameDate { get; set; }
    public RemoteFileDetail? Photo { get; set; }
    public RemoteFileDetail? Video { get; set; }
    public RemoteFileDetail? AlternatePhoto { get; set; }
    public RemoteFileDetail? AlternateVideo { get; set; }
}
