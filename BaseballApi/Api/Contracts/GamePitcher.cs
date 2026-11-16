using BaseballApi.Models;

namespace BaseballApi.Contracts;

public class GamePitcher
{
    public PlayerInfo Player { get; }

    public int Number { get; }
    public int? AppearanceOrder { get; }

    public Dictionary<string, int> Stats { get; }

    public GamePitcher(Pitcher pitcher)
    {
        Player = new PlayerInfo(pitcher.Player);
        Number = pitcher.Number;
        AppearanceOrder = pitcher.AppearanceOrder;
        Stats = new Dictionary<string, int> {
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
        // Only starting pitchers show a game score
        if (AppearanceOrder == null || AppearanceOrder == 1)
        {
            Stats[Stat.GameScore.Name] = GameScore(pitcher);
        }
    }

    private static int GameScore(Pitcher pitcher)
    {
        // Using the Tango definition from https://www.mlb.com/glossary/advanced-stats/game-score
        var gameScore = 40;
        gameScore += pitcher.ThirdInningsPitched * 2;
        gameScore += pitcher.Strikeouts;
        gameScore -= pitcher.Walks * 2;
        gameScore -= pitcher.Hits * 2;
        gameScore -= pitcher.Runs * 3;
        gameScore -= pitcher.Homeruns * 6;
        return gameScore;
    }
}
