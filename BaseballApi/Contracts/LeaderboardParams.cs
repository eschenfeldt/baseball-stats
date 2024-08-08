namespace BaseballApi.Contracts;

public class BatterLeaderboardParams
{
    public int? Year { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public int MinPlateAppearances { get; set; }
    public BatterLeaderboardOrder Order { get; set; }
    public bool SortAscending { get; set; } = false;
}
