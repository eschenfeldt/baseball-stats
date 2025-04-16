using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace BaseballApi.Models;

[Index(nameof(BoxScoreId))]
[Index(nameof(BoxScoreId), nameof(PlayerId), IsUnique = true)]
public class Pitcher
{
    public long Id { get; set; }

    public long BoxScoreId { get; set; }
    public required BoxScore BoxScore { get; set; }

    public long PlayerId { get; set; }
    public required Player Player { get; set; }

    public int Number { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Saves { get; set; }
    [Description("Innings Pitched times 3; i.e. outs recorded")]
    public int ThirdInningsPitched { get; set; }
    public int BattersFaced { get; set; }
    public int Balls { get; set; }
    public int Strikes { get; set; }
    public int Pitches { get; set; }
    public int Runs { get; set; }
    public int EarnedRuns { get; set; }
    public int Hits { get; set; }
    public int Walks { get; set; }
    public int IntentionalWalks { get; set; }
    public int Strikeouts { get; set; }
    public int StrikeoutsCalled { get; set; }
    public int StrikeoutsSwinging { get; set; }
    public int HitByPitch { get; set; }
    public int Balks { get; set; }
    public int WildPitches { get; set; }
    public int Homeruns { get; set; }
    public int GroundOuts { get; set; }
    public int AirOuts { get; set; }
    public int FirstPitchStrikes { get; set; }
    public int FirstPitchBalls { get; set; }
}
