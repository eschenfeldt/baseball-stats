using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BaseballApi.Models;

namespace BaseballApi;

public class BoxScore
{
    public long Id { get; set; }
    public long GameId { get; set; }
    public required Game Game { get; set; }

    public ICollection<Batter> Batters { get; } = [];
    public ICollection<Fielder> Fielders { get; } = [];
    public ICollection<Pitcher> Pitchers { get; } = [];
}
