using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct GamePitcher(Pitcher pitcher)
{
    public PlayerInfo Player { get; set; } = new(pitcher.Player);

    public int Number { get; set; } = pitcher.Number;

    public Dictionary<string, int> Stats { get; } = new() {
        { Stat.Games.Name, pitcher.Games },
        { Stat.Wins.Name, pitcher.Wins },
        { Stat.Losses.Name, pitcher.Losses },
        { Stat.Saves.Name, pitcher.Saves },
        { Stat.ThirdInningsPitched.Name, pitcher.ThirdInningsPitched },
        { Stat.BattersFaced.Name, pitcher.BattersFaced },
        { Stat.Balls.Name, pitcher.Balls },
        { Stat.Strikes.Name, pitcher.Strikes },
        { Stat.Pitches.Name, pitcher.Pitches },
        { Stat.Runs.Name, pitcher.Runs },
        { Stat.EarnedRuns.Name, pitcher.EarnedRuns },
        { Stat.Hits.Name, pitcher.Hits },
        { Stat.Walks.Name, pitcher.Walks },
        { Stat.IntentionalWalks.Name, pitcher.IntentionalWalks },
        { Stat.Strikeouts.Name, pitcher.Strikeouts },
        { Stat.StrikeoutsCalled.Name, pitcher.StrikeoutsCalled },
        { Stat.StrikeoutsSwinging.Name, pitcher.StrikeoutsSwinging },
        { Stat.HitByPitch.Name, pitcher.HitByPitch },
        { Stat.Balks.Name, pitcher.Balks },
        { Stat.WildPitches.Name, pitcher.WildPitches },
        { Stat.Homeruns.Name, pitcher.Homeruns },
        { Stat.GroundOuts.Name, pitcher.GroundOuts },
        { Stat.AirOuts.Name, pitcher.AirOuts },
        { Stat.FirstPitchStrikes.Name, pitcher.FirstPitchStrikes },
        { Stat.FirstPitchBalls.Name, pitcher.FirstPitchBalls },
    };
}
