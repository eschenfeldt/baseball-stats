namespace BaseballApi.Import;

public struct GameImportData
{
    public Dictionary<string, string> FilePaths { get; set; }

    public GameMetadata Metadata { get; set; }
}
