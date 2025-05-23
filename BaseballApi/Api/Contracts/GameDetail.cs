using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct GameDetail(Game game)
{
    public long Id { get; set; } = game.Id;
    public Guid ExternalId { get; set; } = game.ExternalId;
    public string Name { get; set; } = game.Name;
    public DateOnly Date { get; set; } = game.Date;
    public GameType? GameType { get; set; } = game.GameType;
    public Team Home { get; set; } = game.Home;
    public BoxScoreDetail? HomeBoxScore { get; set; } = game.HomeBoxScore != null ? new BoxScoreDetail(game.HomeBoxScore) : null;
    public string HomeTeamName { get; set; } = game.HomeTeamName;
    public Team Away { get; set; } = game.Away;
    public BoxScoreDetail? AwayBoxScore { get; set; } = game.AwayBoxScore != null ? new BoxScoreDetail(game.AwayBoxScore) : null;
    public string AwayTeamName { get; set; } = game.AwayTeamName;
    public DateTimeOffset? ScheduledTime { get; set; } = game.ScheduledTime;
    public DateTimeOffset? StartTime { get; set; } = game.StartTime;
    public DateTimeOffset? EndTime { get; set; } = game.EndTime;
    public Park? Location { get; set; } = game.Location;
    public ScorecardDetail? Scorecard { get; } = game.Scorecard != null ? new(game.Scorecard) : null;
    public bool HasMedia { get; } = game.Media.Count != 0;
    public int? HomeScore { get; set; } = game.HomeScore;
    public int? AwayScore { get; set; } = game.AwayScore;
    public Team? WinningTeam { get; set; } = game.WinningTeam;
    public Team? LosingTeam { get; set; } = game.LosingTeam;
    public Player? WinningPitcher { get; set; } = game.WinningPitcher;
    public Player? LosingPitcher { get; set; } = game.LosingPitcher;
    public Player? SavingPitcher { get; set; } = game.SavingPitcher;

    public IReadOnlyDictionary<string, Stat> Stats { get; set; } = StatCollection.GameStats;
}
