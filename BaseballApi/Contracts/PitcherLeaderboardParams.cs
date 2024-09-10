namespace BaseballApi.Contracts;

public struct PitcherLeaderboardParams : ILeaderboardParams
{
    public PitcherLeaderboardParams()
    {
    }

    public int? Year { get; set; }
    public string? PlayerSearch { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public int MinInningsPitched { get; set; } = 0;
    public string Sort { get; set; } = Stat.ThirdInningsPitched.Name;
    public bool Asc { get; set; } = false;
}
