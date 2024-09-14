using System;
using System.Reflection;

namespace BaseballApi.Contracts;

public class StatCollection
{
    public static readonly StatCollection Instance = new();
    public readonly IReadOnlyDictionary<string, Stat> Stats;

    private StatCollection()
    {
        var statProps = typeof(Stat).GetFields(BindingFlags.Static | BindingFlags.Public);
        var stats = new Dictionary<string, Stat>();
        foreach (var statProp in statProps)
        {
            if (statProp.FieldType == typeof(Stat))
            {
                var stat = statProp.GetValue(null);
                if (stat != null)
                {
                    var statVal = (Stat)stat;
                    stats[statVal.Name] = statVal;
                }
            }
        }
        Stats = stats;
    }

    private static readonly List<string> GameStatNames = [
        Stat.Games.Name,
        Stat.PlateAppearances.Name,
        Stat.AtBats.Name,
        Stat.Runs.Name,
        Stat.Hits.Name,
        Stat.BuntSingles.Name,
        Stat.Singles.Name,
        Stat.Doubles.Name,
        Stat.Triples.Name,
        Stat.Homeruns.Name,
        Stat.RunsBattedIn.Name,
        Stat.Walks.Name,
        Stat.Strikeouts.Name,
        Stat.StrikeoutsCalled.Name,
        Stat.StrikeoutsSwinging.Name,
        Stat.HitByPitch.Name,
        Stat.StolenBases.Name,
        Stat.CaughtStealing.Name,
        Stat.SacrificeBunts.Name,
        Stat.SacrificeFlies.Name,
        Stat.Sacrifices.Name,
        Stat.ReachedOnError.Name,
        Stat.FieldersChoices.Name,
        Stat.CatchersInterference.Name,
        Stat.GroundedIntoDoublePlay.Name,
        Stat.GroundedIntoTriplePlay.Name,
        Stat.AtBatsWithRunnersInScoringPosition.Name,
        Stat.HitsWithRunnersInScoringPosition.Name,
        Stat.Wins.Name,
        Stat.Losses.Name,
        Stat.Saves.Name,
        Stat.ThirdInningsPitched.Name,
        Stat.BattersFaced.Name,
        Stat.Balls.Name,
        Stat.Strikes.Name,
        Stat.Pitches.Name,
        Stat.EarnedRuns.Name,
        Stat.IntentionalWalks.Name,
        Stat.Balks.Name,
        Stat.WildPitches.Name,
        Stat.GroundOuts.Name,
        Stat.AirOuts.Name,
        Stat.FirstPitchStrikes.Name,
        Stat.FirstPitchBalls.Name,
        Stat.Errors.Name,
        Stat.ErrorsThrowing.Name,
        Stat.ErrorsFielding.Name,
        Stat.Putouts.Name,
        Stat.Assists.Name,
        Stat.StolenBaseAttempts.Name,
        Stat.DoublePlays.Name,
        Stat.TriplePlays.Name,
        Stat.PassedBalls.Name,
        Stat.PickoffFailed.Name,
        Stat.PickoffSuccess.Name
    ];
    public static readonly IReadOnlyDictionary<string, Stat> GameStats = Instance.Stats.Where(kvp => GameStatNames.Contains(kvp.Key)).ToDictionary();
}
