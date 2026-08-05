using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Import;

public class GameImportManager(GameImportData data, BaseballContext context)
{
    BaseballContext Context { get; } = context;
    GameImportData Data { get; } = data;
    GameMetadata Metadata => Data.Metadata;
    Dictionary<ImportFileType, CsvLoader> Files { get; } = [];
    private PlayerManager PlayerManager { get; } = new PlayerManager(context);
    public string? ScorecardFilePath { get; private set; }
    private TimeZoneInfo? ParkTimeZone { get; set; }
    private DateOnly GameDate
    {
        get
        {
            // Match the iScore importer precedence for now, see #154
            var start = Metadata.ScheduledStart ?? Metadata.ActualStart;
            if (!start.HasValue)
            {
                throw new ArgumentException("Must provide either actual or scheduled start time");
            }
            var parkTime = ParkTimeZone == null
                ? start.Value
                : TimeZoneInfo.ConvertTime(start.Value, ParkTimeZone);
            return DateOnly.FromDateTime(parkTime.Date);
        }
    }

    public async Task<Game> GetGame()
    {
        var home = await this.GetTeam(Metadata.Home.City, Metadata.Home.Name);
        var away = await this.GetTeam(Metadata.Away.City, Metadata.Away.Name);
        this.ParkTimeZone = GetTimeZone(home.HomePark);
        var gameDate = GameDate;
        var awayTeamName = $"{Metadata.Away.City} {Metadata.Away.Name}";
        var homeTeamName = $"{Metadata.Home.City} {Metadata.Home.Name}";
        var gameName = $"{gameDate:M/d/yy} {awayTeamName} at {homeTeamName}";
        return new Game
        {
            Name = gameName,
            Date = gameDate,
            // always store times with zero offset (because that's all Postgres accepts)
            ScheduledTime = Metadata.ScheduledStart?.ToUniversalTime(),
            StartTime = Metadata.ActualStart?.ToUniversalTime(),
            EndTime = Metadata.End?.ToUniversalTime(),
            GameType = Metadata.GameType,
            HomeTeamName = homeTeamName,
            Home = home,
            AwayTeamName = awayTeamName,
            Away = away,
            Scorecard = this.GetScorecard(),
            BoxScores = []
        };
    }

    private static TimeZoneInfo? GetTimeZone(Park? park)
    {
        if (park?.TimeZone is string timeZoneId
            && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out TimeZoneInfo? timeZone))
        {
            return timeZone;
        }
        return null;
    }

    public void AddLocation(Game game)
    {
        if (game.Home != null && game.Home.HomePark != null)
        {
            game.Location = game.Home.HomePark;
        }
    }

    public void AddPitcherDecisions(Game game)
    {
        if (game.WinningTeam == game.Home && game.HomeBoxScore != null)
        {
            this.AddWinningTeamDecisions(game, game.HomeBoxScore);
        }
        else if (game.WinningTeam == game.Away && game.AwayBoxScore != null)
        {
            this.AddWinningTeamDecisions(game, game.AwayBoxScore);
        }

        if (game.LosingTeam == game.Home && game.HomeBoxScore != null)
        {
            this.AddLosingTeamDecisions(game, game.HomeBoxScore);
        }
        else if (game.LosingTeam == game.Away && game.AwayBoxScore != null)
        {
            this.AddLosingTeamDecisions(game, game.AwayBoxScore);
        }
    }

    private void AddWinningTeamDecisions(Game game, BoxScore winningBoxScore)
    {
        game.WinningPitcher = winningBoxScore.Pitchers.FirstOrDefault(p => p.Wins > 0)?.Player;
        game.SavingPitcher = winningBoxScore.Pitchers.FirstOrDefault(p => p.Saves > 0)?.Player;
    }

    private void AddLosingTeamDecisions(Game game, BoxScore losingBoxScore)
    {
        game.LosingPitcher = losingBoxScore.Pitchers.FirstOrDefault(p => p.Losses > 0)?.Player;
    }

    private async Task<Team> GetTeam(string city, string name)
    {
        var altName = $"{city} {name}";
        var existing = await Context.Teams.Include(t => t.HomePark).FirstOrDefaultAsync(t =>
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
            batter.Player = this.GetOrAddPlayer(batter.Player, boxScore.TeamId, batter.Number);
            boxScore.Batters.Add(batter);
        }
        foreach (var pitcher in this.GetPitchers(boxScore, home))
        {
            pitcher.Player = this.GetOrAddPlayer(pitcher.Player, boxScore.TeamId, pitcher.Number);
            boxScore.Pitchers.Add(pitcher);
        }
        foreach (var fielder in this.GetFielders(boxScore, home))
        {
            fielder.Player = this.GetOrAddPlayer(fielder.Player, boxScore.TeamId, fielder.Number);
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

    private Player GetOrAddPlayer(Player player, long teamId, int number)
    {
        return this.PlayerManager.GetOrCreatePlayer(player.Name, teamId, number, GameDate.Year);
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
