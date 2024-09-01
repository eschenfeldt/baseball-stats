namespace BaseballApi.Contracts;

public struct LeaderboardBatter
{
    public required PlayerInfo Player { get; set; }
    public int? Year { get; set; }
    public Dictionary<string, decimal?> Stats { get; set; }
}

public enum BatterLeaderboardOrder
{
    Games,
    BattingAverage
}
