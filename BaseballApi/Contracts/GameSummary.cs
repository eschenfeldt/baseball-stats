using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct GameSummary(Game game)
{
    public long Id { get; set; } = game.Id;
    public Guid ExternalId { get; set; } = game.ExternalId;
    public string Name { get; set; } = game.Name;
    public DateOnly Date { get; set; } = game.Date;
    public GameType? GameType { get; set; } = game.GameType;
    public Team Home { get; set; } = game.Home;
    public string HomeTeamName { get; set; } = game.HomeTeamName;
    public Team Away { get; set; } = game.Away;
    public string AwayTeamName { get; set; } = game.AwayTeamName;
    public DateTimeOffset? ScheduledTime { get; set; } = game.ScheduledTime;
    public DateTimeOffset? StartTime { get; set; } = game.StartTime;
    public DateTimeOffset? EndTime { get; set; } = game.EndTime;
    public Park? Location { get; set; } = game.Location;
    public int? HomeScore { get; set; } = game.HomeScore;
    public int? AwayScore { get; set; } = game.AwayScore;
    public Team? WinningTeam { get; set; } = game.WinningTeam;
    public Team? LosingTeam { get; set; } = game.LosingTeam;
    public PlayerInfo? WinningPitcher { get; set; } = game.WinningPitcher == null ? null : new(game.WinningPitcher);
    public PlayerInfo? LosingPitcher { get; set; } = game.LosingPitcher == null ? null : new(game.LosingPitcher);
    public PlayerInfo? SavingPitcher { get; set; } = game.SavingPitcher == null ? null : new(game.SavingPitcher);
}

public enum GameOrder
{
    [ParamValue("date")]
    Date
}