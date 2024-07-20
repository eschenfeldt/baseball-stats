using BaseballApi.Models;

namespace BaseballApi;

public class GameImportManager
{
    GameImportData Data { get; }
    GameMetadata Metadata => Data.Metadata;
    Dictionary<ImportFileType, CsvLoader> Files { get; }
    public GameImportManager(GameImportData data)
    {
        this.Data = data;
        this.Files = new Dictionary<ImportFileType, CsvLoader>();
    }

    public Game GetGame()
    {
        var homeBatters = this.GetOrLoadFile(ImportFileType.HomeBatting);

        var gameDate = Metadata.ActualStart?.Date ?? Metadata.ScheduledStart?.Date;
        if (!gameDate.HasValue)
        {
            throw new ArgumentException("Must provide either actual or scheduled start time");
        }
        var awayTeamName = $"{Metadata.Away.City} {Metadata.Away.Name}";
        var homeTeamName = $"{Metadata.Home.City} {Metadata.Home.Name}";
        var gameName = $"{gameDate:M/d/yy} {awayTeamName} at {homeTeamName}";
        return new Game
        {
            Name = gameName,
            Date = DateOnly.FromDateTime(gameDate.Value),
            // always store times with zero offset (because that's all Postgres accepts)
            ScheduledTime = Metadata.ScheduledStart?.ToUniversalTime(),
            StartTime = Metadata.ActualStart?.ToUniversalTime(),
            EndTime = Metadata.End?.ToUniversalTime(),
            HomeTeamName = homeTeamName,
            Home = new Team
            {
                City = Metadata.Home.City,
                Name = Metadata.Home.Name
            },
            AwayTeamName = awayTeamName,
            Away = new Team
            {
                City = Metadata.Away.City,
                Name = Metadata.Away.Name
            },
            BoxScores = []
        };
    }

    private CsvLoader GetOrLoadFile(ImportFileType fileType)
    {
        if (this.Files.TryGetValue(fileType, out CsvLoader? file))
        {
            return file;
        }
        else if (this.Data.FilePaths.TryGetValue(fileType.ExpectedFileName(), out string? filePath))
        {
            var loader = new CsvLoader(filePath);
            loader.LoadData();
            this.Files[fileType] = loader;
            return loader;
        }
        else
        {
            throw new ArgumentException($"No '{fileType.ExpectedFileName()}' file found");
        }
    }
}
