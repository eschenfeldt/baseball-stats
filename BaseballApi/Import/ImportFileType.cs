namespace BaseballApi.Import;

public enum ImportFileType
{
    HomeBatting,
    HomeFielding,
    HomePitching,
    VisitorBatting,
    VisitorFielding,
    VisitorPitching,
    Scorecard
}

public static class ImportFileTypeExtensions
{
    public static string ExpectedFileName(this ImportFileType fileType)
    {
        return fileType switch
        {
            ImportFileType.HomeBatting => "statsHomeBatting.csv",
            ImportFileType.HomeFielding => "statsHomeFielding.csv",
            ImportFileType.HomePitching => "statsHomePitching.csv",
            ImportFileType.VisitorBatting => "statsVisitorBatting.csv",
            ImportFileType.VisitorFielding => "statsVisitorFielding.csv",
            ImportFileType.VisitorPitching => "statsVisitorPitching.csv",
            ImportFileType.Scorecard => "scorecard.pdf",
            _ => throw new ArgumentOutOfRangeException($"Unexpected file type '{fileType}'"),
        };
    }
}
