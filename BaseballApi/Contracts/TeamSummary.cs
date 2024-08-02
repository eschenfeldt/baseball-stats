namespace BaseballApi;

public class TeamSummary
{
    public required Team Team { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
}
