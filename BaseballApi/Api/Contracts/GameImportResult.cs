using System;
using BaseballApi.Import;

namespace BaseballApi.Contracts;

public struct GameImportResult
{
    public long Id { get; set; }
    public int Count { get; set; }
    public long Size { get; set; }
    public GameMetadata Metadata { get; set; }
    public int Changes { get; set; }
}
