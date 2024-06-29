using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace BaseballApi.Models;

public class Game
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public DateOnly Date { get; set; }
    public GameType GameType { get; set; }
    public required Team Home { get; set; }
    public required Team Away { get; set; }
    public DateTimeOffset ScheduledTime { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public Park? Location { get; set; }
    public BoxScore? BoxScore { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public Team? WinningTeam { get; set; }
    public Player? WinningPitcher { get; set; }
    public Player? LosingPitcher { get; set; }
    public Player? SavingPitcher { get; set; }
}