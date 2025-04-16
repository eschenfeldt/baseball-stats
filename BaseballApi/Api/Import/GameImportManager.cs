using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Import;

public class GameImportManager
{
    BaseballContext Context { get; }
    GameImportData Data { get; }
    GameMetadata Metadata => Data.Metadata;
    Dictionary<ImportFileType, CsvLoader> Files { get; }
    Dictionary<string, Player> NewPlayers { get; } = [];
    public string? ScorecardFilePath { get; private set; }
    public GameImportManager(GameImportData data, BaseballContext context)
    {
        this.Context = context;
        this.Data = data;
        this.Files = new Dictionary<ImportFileType, CsvLoader>();
    }

    public async Task<Game> GetGame()
    {
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
            Home = await this.GetTeam(Metadata.Home.City, Metadata.Home.Name),
            AwayTeamName = awayTeamName,
            Away = await this.GetTeam(Metadata.Away.City, Metadata.Away.Name),
            Scorecard = this.GetScorecard(),
            BoxScores = []
        };
    }

    private async Task<Team> GetTeam(string city, string name)
    {
        var altName = $"{city} {name}";
        var existing = await Context.Teams.FirstOrDefaultAsync(t =>
            t.City == city && t.Name == name
            || t.AlternateTeamNames.Any(atn => atn.FullName == altName)
        );
        if (existing != null)
        {
            return existing;
        }
        else
        {
            return new Team
            {
                City = city,
                Name = name
            };
        }
    }

    public void PopulateBoxScore(BoxScore boxScore, bool home)
    {
        foreach (var batter in this.GetBatters(boxScore, home))
        {
            batter.Player = this.GetOrAddPlayer(batter.Player);
            boxScore.Batters.Add(batter);
        }
        foreach (var pitcher in this.GetPitchers(boxScore, home))
        {
            pitcher.Player = this.GetOrAddPlayer(pitcher.Player);
            boxScore.Pitchers.Add(pitcher);
        }
        foreach (var fielder in this.GetFielders(boxScore, home))
        {
            fielder.Player = this.GetOrAddPlayer(fielder.Player);
            boxScore.Fielders.Add(fielder);
        }
    }

    private IEnumerable<Batter> GetBatters(BoxScore boxScore, bool home)
    {
        var fileType = home ? ImportFileType.HomeBatting : ImportFileType.VisitorBatting;
        var stats = this.GetOrLoadFile(fileType);
        return stats.GetBatters(boxScore);
    }

    private IEnumerable<Pitcher> GetPitchers(BoxScore boxScore, bool home)
    {
        var fileType = home ? ImportFileType.HomePitching : ImportFileType.VisitorPitching;
        var stats = this.GetOrLoadFile(fileType);
        return stats.GetPitchers(boxScore);
    }

    private IEnumerable<Fielder> GetFielders(BoxScore boxScore, bool home)
    {
        var fileType = home ? ImportFileType.HomeFielding : ImportFileType.VisitorFielding;
        var stats = this.GetOrLoadFile(fileType);
        return stats.GetFielders(boxScore);
    }

    private Player GetOrAddPlayer(Player player)
    {
        var existingPlayer = this.Context.Players.FirstOrDefault(p => p.Name == player.Name);
        if (existingPlayer != null)
        {
            return existingPlayer;
        }
        else if (this.NewPlayers.TryGetValue(player.Name, out Player? newPlayer))
        {
            return newPlayer;
        }
        else
        {
            this.NewPlayers[player.Name] = player;
            return player;
        }
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

    private Scorecard? GetScorecard()
    {
        if (this.Data.FilePaths.TryGetValue(ImportFileType.Scorecard.ExpectedFileName(), out string? filePath))
        {
            var originalFileName = ImportFileType.Scorecard.ExpectedFileName();
            var scorecard = new Scorecard
            {
                AssetIdentifier = Guid.NewGuid(),
                OriginalFileName = originalFileName
            };
            if (this.Data.Metadata.End.HasValue)
            {
                scorecard.DateTime = this.Data.Metadata.End.Value.ToUniversalTime();
            }
            var extension = Path.GetExtension(originalFileName);
            var file = new RemoteFile
            {
                Resource = scorecard,
                Purpose = RemoteFilePurpose.Original,
                Extension = extension
            };
            scorecard.Files.Add(file);
            this.ScorecardFilePath = filePath;
            return scorecard;
        }
        else
        {
            return null;
        }
    }
}
