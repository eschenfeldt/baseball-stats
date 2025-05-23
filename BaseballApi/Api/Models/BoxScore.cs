using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BaseballApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Models;

[Index(nameof(GameId))]
[Index(nameof(GameId), nameof(TeamId), IsUnique = true)]
public class BoxScore
{
    public long Id { get; set; }
    public long GameId { get; set; }
    [InverseProperty(nameof(Game.BoxScores))]
    [ForeignKey(nameof(GameId))]
    public required Game Game { get; set; }

    public long TeamId { get; set; }
    public required Team Team { get; set; }

    public ICollection<Batter> Batters { get; } = [];
    public ICollection<Fielder> Fielders { get; } = [];
    public ICollection<Pitcher> Pitchers { get; } = [];
}
