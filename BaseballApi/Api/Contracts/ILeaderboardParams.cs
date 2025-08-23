namespace BaseballApi.Contracts;

public interface ILeaderboardParams
{
    public int? Year { get; set; }
    public long? TeamId { get; set; }
    public long? ParkId { get; set; }
    public long? PlayerId { get; set; }
    public string? PlayerSearch { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    public bool Asc { get; set; }
}
