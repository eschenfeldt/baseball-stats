using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct BoxScoreDetail(BoxScore boxScore)
{
    public Team Team { get; set; } = boxScore.Team;

    public List<GameBatter> Batters { get; set; } = boxScore.Batters.Select(b => new GameBatter(b))
        // Players with unknown lineup position or appearance order go last
        .OrderBy(b => b.LineupPosition ?? int.MaxValue)
        .ThenBy(b => b.AppearanceOrder ?? int.MaxValue)
        .ToList();

    public List<GamePitcher> Pitchers { get; set; } = boxScore.Pitchers.Select(p => new GamePitcher(p))
        .OrderBy(p => p.AppearanceOrder ?? int.MaxValue)
        .ToList();

    public List<GameFielder> Fielders { get; set; } = boxScore.Fielders.Select(f => new GameFielder(f)).ToList();
}
