namespace BaseballApi;

public class Fielder
{
    public long Id { get; set; }

    public long BoxScoreId { get; set; }
    public required BoxScore BoxScore { get; set; }

    public long PlayerId { get; set; }
    public required Player Player { get; set; }

    public int Number { get; set; }
    public int Games { get; set; }
    public int Errors { get; set; }
    public int ErrorsThrowing { get; set; }
    public int ErrorsFielding { get; set; }
    public int Putouts { get; set; }
    public int Assists { get; set; }
    public int StolenBaseAttempts { get; set; }
    public int CaughtStealing { get; set; }
    public int DoublePlays { get; set; }
    public int TriplePlays { get; set; }
    public int PassedBalls { get; set; }
    public int PickoffFailed { get; set; }
    public int PickoffSuccess { get; set; }
}
