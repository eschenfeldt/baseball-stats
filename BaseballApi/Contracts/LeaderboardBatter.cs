namespace BaseballApi.Contracts;

public class LeaderboardBatter
{
    public required Player Player { get; set; }
    public int? Year { get; set; }
    public int Games { get; set; }
    public int AtBats { get; set; }
    public int Hits { get; set; }
    public decimal? BattingAverage { get; set; }
}

public enum BatterLeaderboardOrder
{
    Games,
    BattingAverage
}
