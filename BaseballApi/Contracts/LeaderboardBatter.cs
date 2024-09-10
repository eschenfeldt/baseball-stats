namespace BaseballApi.Contracts;

public struct LeaderboardPlayer
{
    public required PlayerInfo Player { get; set; }
    public int? Year { get; set; }
    public Dictionary<string, decimal?> Stats { get; set; }
}
