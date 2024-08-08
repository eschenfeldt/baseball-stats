namespace BaseballApi.Contracts;

public struct BoxScoreDetail(BoxScore boxScore)
{
    public Team Team { get; set; } = boxScore.Team;

    public List<GameBatter> Batters { get; set; } = boxScore.Batters.Select(b => new GameBatter(b)).ToList();

    public List<GamePitcher> Pitchers { get; set; } = boxScore.Pitchers.Select(p => new GamePitcher(p)).ToList();

    public List<GameFielder> Fielders { get; set; } = boxScore.Fielders.Select(f => new GameFielder(f)).ToList();
}
