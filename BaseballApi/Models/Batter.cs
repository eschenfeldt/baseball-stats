using System.ComponentModel.DataAnnotations.Schema;
using BaseballApi.Models;

namespace BaseballApi;

public class Batter
{
    public long Id { get; set; }

    public long BoxScoreId { get; set; }
    public required BoxScore BoxScore { get; set; }

    public long PlayerId { get; set; }
    public required Player Player { get; set; }

    public int Number { get; set; }
    public int Games { get; set; }
    public int PlateAppearances { get; set; }
    public int AtBats { get; set; }
    public int Runs { get; set; }
    public int Hits { get; set; }
    public int BuntSingles { get; set; }
    public int Singles { get; set; }
    public int Doubles { get; set; }
    public int Triples { get; set; }
    public int Homeruns { get; set; }
    public int RunsBattedIn { get; set; }
    public int Walks { get; set; }
    public int Strikeouts { get; set; }
    public int StrikeoutsCalled { get; set; }
    public int StrikeoutsSwinging { get; set; }
    public int HitByPitch { get; set; }
    public int StolenBases { get; set; }
    public int CaughtStealing { get; set; }
    public int SacrificeBunts { get; set; }
    public int SacrificeFlies { get; set; }
    public int Sacrifices { get; set; }
    public int ReachedOnError { get; set; }
    public int FieldersChoices { get; set; }
    public int CatchersInterference { get; set; }
    public int GroundedIntoDoublePlay { get; set; }
    public int GroundedIntoTriplePlay { get; set; }
    public int AtBatsWithRunnersInScoringPosition { get; set; }
    public int HitsWithRunnersInScoringPosition { get; set; }
}
