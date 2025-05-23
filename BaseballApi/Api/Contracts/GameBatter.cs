using BaseballApi.Models;

namespace BaseballApi.Contracts;

public class GameBatter(Batter batter)
{
    public PlayerInfo Player { get; set; } = new(batter.Player);

    public int Number { get; set; } = batter.Number;

    public Dictionary<string, int> Stats { get; set; } = new()
    {
        { Stat.Games.Name, batter.Games },
        { Stat.PlateAppearances.Name, batter.PlateAppearances },
        { Stat.AtBats.Name, batter.AtBats },
        { Stat.Runs.Name, batter.Runs },
        { Stat.Hits.Name, batter.Hits },
        { Stat.BuntSingles.Name, batter.BuntSingles },
        { Stat.Singles.Name, batter.Singles },
        { Stat.Doubles.Name, batter.Doubles },
        { Stat.Triples.Name, batter.Triples },
        { Stat.Homeruns.Name, batter.Homeruns },
        { Stat.RunsBattedIn.Name, batter.RunsBattedIn },
        { Stat.Walks.Name, batter.Walks },
        { Stat.Strikeouts.Name, batter.Strikeouts },
        { Stat.StrikeoutsCalled.Name, batter.StrikeoutsCalled },
        { Stat.StrikeoutsSwinging.Name, batter.StrikeoutsSwinging },
        { Stat.HitByPitch.Name, batter.HitByPitch },
        { Stat.StolenBases.Name, batter.StolenBases },
        { Stat.CaughtStealing.Name, batter.CaughtStealing },
        { Stat.SacrificeBunts.Name, batter.SacrificeBunts },
        { Stat.SacrificeFlies.Name, batter.SacrificeFlies },
        { Stat.Sacrifices.Name, batter.Sacrifices },
        { Stat.ReachedOnError.Name, batter.ReachedOnError },
        { Stat.FieldersChoices.Name, batter.FieldersChoices },
        { Stat.CatchersInterference.Name, batter.CatchersInterference },
        { Stat.GroundedIntoDoublePlay.Name, batter.GroundedIntoDoublePlay },
        { Stat.GroundedIntoTriplePlay.Name, batter.GroundedIntoTriplePlay },
        { Stat.AtBatsWithRunnersInScoringPosition.Name, batter.AtBatsWithRunnersInScoringPosition },
        { Stat.HitsWithRunnersInScoringPosition.Name, batter.HitsWithRunnersInScoringPosition },
    };
}
