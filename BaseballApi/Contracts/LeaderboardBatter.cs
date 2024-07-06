namespace BaseballApi;

public class LeaderboardBatter
{
    public required Player Player { get; set; }
    public int? Year { get; set; }
    public int Games { get; set; }
    public decimal BattingAverage { get; set; }
}
