namespace BaseballApi.Contracts;

public struct GamePitcher(Pitcher pitcher)
{
    public Player Player { get; set; } = pitcher.Player;

    public int Number { get; set; } = pitcher.Number;
    public int Games { get; set; } = pitcher.Games;
    public int Wins { get; set; } = pitcher.Wins;
    public int Losses { get; set; } = pitcher.Losses;
    public int Saves { get; set; } = pitcher.Saves;
    public int ThirdInningsPitched { get; set; } = pitcher.ThirdInningsPitched;
    public int BattersFaced { get; set; } = pitcher.BattersFaced;
    public int Balls { get; set; } = pitcher.Balls;
    public int Strikes { get; set; } = pitcher.Strikes;
    public int Pitches { get; set; } = pitcher.Pitches;
    public int Runs { get; set; } = pitcher.Runs;
    public int EarnedRuns { get; set; } = pitcher.EarnedRuns;
    public int Hits { get; set; } = pitcher.Hits;
    public int Walks { get; set; } = pitcher.Walks;
    public int IntentionalWalks { get; set; } = pitcher.IntentionalWalks;
    public int Strikeouts { get; set; } = pitcher.Strikeouts;
    public int StrikeoutsCalled { get; set; } = pitcher.StrikeoutsCalled;
    public int StrikeoutsSwinging { get; set; } = pitcher.StrikeoutsSwinging;
    public int HitByPitch { get; set; } = pitcher.HitByPitch;
    public int Balks { get; set; } = pitcher.Balks;
    public int WildPitches { get; set; } = pitcher.WildPitches;
    public int Homeruns { get; set; } = pitcher.Homeruns;
    public int GroundOuts { get; set; } = pitcher.GroundOuts;
    public int AirOuts { get; set; } = pitcher.AirOuts;
    public int FirstPitchStrikes { get; set; } = pitcher.FirstPitchStrikes;
    public int FirstPitchBalls { get; set; } = pitcher.FirstPitchBalls;
}
