namespace BaseballApi.Contracts;

public struct BatterLeaderboardParams : ILeaderboardParams
{
    public BatterLeaderboardParams()
    {
    }

    public int? Year { get; set; }
    public string? PlayerSearch { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public int MinPlateAppearances { get; set; } = 0;
    public string Sort { get; set; } = Stat.Games.Name;
    public bool Asc { get; set; } = false;
}
