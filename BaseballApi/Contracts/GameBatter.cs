namespace BaseballApi.Contracts;

public struct GameBatter(Batter batter)
{
    public Player Player { get; set; } = batter.Player;

    public int Number { get; set; } = batter.Number;
    public int Games { get; set; } = batter.Games;
    public int PlateAppearances { get; set; } = batter.PlateAppearances;
    public int AtBats { get; set; } = batter.AtBats;
    public int Runs { get; set; } = batter.Runs;
    public int Hits { get; set; } = batter.Hits;
    public int BuntSingles { get; set; } = batter.BuntSingles;
    public int Singles { get; set; } = batter.Singles;
    public int Doubles { get; set; } = batter.Doubles;
    public int Triples { get; set; } = batter.Triples;
    public int Homeruns { get; set; } = batter.Homeruns;
    public int RunsBattedIn { get; set; } = batter.RunsBattedIn;
    public int Walks { get; set; } = batter.Walks;
    public int Strikeouts { get; set; } = batter.Strikeouts;
    public int StrikeoutsCalled { get; set; } = batter.StrikeoutsCalled;
    public int StrikeoutsSwinging { get; set; } = batter.StrikeoutsSwinging;
    public int HitByPitch { get; set; } = batter.HitByPitch;
    public int StolenBases { get; set; } = batter.StolenBases;
    public int CaughtStealing { get; set; } = batter.CaughtStealing;
    public int SacrificeBunts { get; set; } = batter.SacrificeBunts;
    public int SacrificeFlies { get; set; } = batter.SacrificeFlies;
    public int Sacrifices { get; set; } = batter.Sacrifices;
    public int ReachedOnError { get; set; } = batter.ReachedOnError;
    public int FieldersChoices { get; set; } = batter.FieldersChoices;
    public int CatchersInterference { get; set; } = batter.CatchersInterference;
    public int GroundedIntoDoublePlay { get; set; } = batter.GroundedIntoDoublePlay;
    public int GroundedIntoTriplePlay { get; set; } = batter.GroundedIntoTriplePlay;
    public int AtBatsWithRunnersInScoringPosition { get; set; } = batter.AtBatsWithRunnersInScoringPosition;
    public int HitsWithRunnersInScoringPosition { get; set; } = batter.HitsWithRunnersInScoringPosition;
}
