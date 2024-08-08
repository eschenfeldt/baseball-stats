namespace BaseballApi.Contracts;

public class TeamSummary
{
    public required Team Team { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public DateOnly? LastGameDate { get; set; }
}

public enum TeamSummaryOrder
{
    [ParamValue("games")]
    Games,
    [ParamValue("team")]
    Team,
    [ParamValue("wins")]
    Wins,
    [ParamValue("losses")]
    Losses,
    [ParamValue("lastGame")]
    LastGame
}