namespace BaseballApi;

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
        switch (fileType)
        {
            case ImportFileType.HomeBatting:
                return "statsHomeBatting.csv";
            case ImportFileType.HomeFielding:
                return "statsHomeFielding.csv";
            case ImportFileType.HomePitching:
                return "statsHomePitching.csv";
            case ImportFileType.VisitorBatting:
                return "statsVisitorBatting.csv";
            case ImportFileType.VisitorFielding:
                return "statsVisitorFielding.csv";
            case ImportFileType.VisitorPitching:
                return "statsVisitorPitching.csv";
            case ImportFileType.Scorecard:
                return "scorecard.pdf";
            default:
                throw new ArgumentOutOfRangeException($"Unexpected file type '{fileType}'");
        }
    }
}
