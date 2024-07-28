using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Models;

[Index(nameof(ExternalId), IsUnique = true)]
public class Game
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public required string Name { get; set; }
    public DateOnly Date { get; set; }
    public GameType? GameType { get; set; }
    public required Team Home { get; set; }
    public required string HomeTeamName { get; set; }
    public required Team Away { get; set; }
    public required string AwayTeamName { get; set; }
    public DateTimeOffset? ScheduledTime { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public Park? Location { get; set; }
    public required ICollection<BoxScore> BoxScores { get; set; }
    public long? HomeBoxScoreId { get; set; }
    [ForeignKey(nameof(HomeBoxScoreId))]
    public BoxScore? HomeBoxScore { get; set; }
    public long? AwayBoxScoreId { get; set; }
    [ForeignKey(nameof(AwayBoxScoreId))]
    public BoxScore? AwayBoxScore { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public Team? WinningTeam { get; set; }
    public Team? LosingTeam { get; set; }
    public Player? WinningPitcher { get; set; }
    public Player? LosingPitcher { get; set; }
    public Player? SavingPitcher { get; set; }
}