using BaseballApi.Models;

namespace BaseballApi.Contracts;

public struct GameDetail(Game game)
{
    public long Id { get; set; } = game.Id;
    public Guid ExternalId { get; set; } = game.ExternalId;
    public string Name { get; set; } = game.Name;
    public DateOnly Date { get; set; } = game.Date;
    public GameType? GameType { get; set; } = game.GameType;
    public Team Home { get; set; } = game.Home;
    public BoxScoreDetail? HomeBoxScore { get; set; } = game.HomeBoxScore != null ? new BoxScoreDetail(game.HomeBoxScore) : null;
    public string HomeTeamName { get; set; } = game.HomeTeamName;
    public Team Away { get; set; } = game.Away;
    public BoxScoreDetail? AwayBoxScore { get; set; } = game.AwayBoxScore != null ? new BoxScoreDetail(game.AwayBoxScore) : null;
    public string AwayTeamName { get; set; } = game.AwayTeamName;
    public DateTimeOffset? ScheduledTime { get; set; } = game.ScheduledTime;
    public DateTimeOffset? StartTime { get; set; } = game.StartTime;
    public DateTimeOffset? EndTime { get; set; } = game.EndTime;
    public Park? Location { get; set; } = game.Location;
    public ScorecardDetail? Scorecard { get; } = game.Scorecard != null ? new(game.Scorecard) : null;
    public bool HasMedia { get; } = game.Media.Count != 0;
    public int? HomeScore { get; set; } = game.HomeScore;
    public int? AwayScore { get; set; } = game.AwayScore;
    public Team? WinningTeam { get; set; } = game.WinningTeam;
    public Team? LosingTeam { get; set; } = game.LosingTeam;
    public Player? WinningPitcher { get; set; } = game.WinningPitcher;
    public Player? LosingPitcher { get; set; } = game.LosingPitcher;
    public Player? SavingPitcher { get; set; } = game.SavingPitcher;

    public Dictionary<string, Stat> Stats { get; set; } = new()
    {
        { Stat.Games.Name, Stat.Games },
        { Stat.PlateAppearances.Name, Stat.PlateAppearances },
        { Stat.AtBats.Name, Stat.AtBats },
        { Stat.Runs.Name, Stat.Runs },
        { Stat.Hits.Name, Stat.Hits },
        { Stat.BuntSingles.Name, Stat.BuntSingles },
        { Stat.Singles.Name, Stat.Singles },
        { Stat.Doubles.Name, Stat.Doubles },
        { Stat.Triples.Name, Stat.Triples },
        { Stat.Homeruns.Name, Stat.Homeruns },
        { Stat.RunsBattedIn.Name, Stat.RunsBattedIn },
        { Stat.Walks.Name, Stat.Walks },
        { Stat.Strikeouts.Name, Stat.Strikeouts },
        { Stat.StrikeoutsCalled.Name, Stat.StrikeoutsCalled },
        { Stat.StrikeoutsSwinging.Name, Stat.StrikeoutsSwinging },
        { Stat.HitByPitch.Name, Stat.HitByPitch },
        { Stat.StolenBases.Name, Stat.StolenBases },
        { Stat.CaughtStealing.Name, Stat.CaughtStealing },
        { Stat.SacrificeBunts.Name, Stat.SacrificeBunts },
        { Stat.SacrificeFlies.Name, Stat.SacrificeFlies },
        { Stat.Sacrifices.Name, Stat.Sacrifices },
        { Stat.ReachedOnError.Name, Stat.ReachedOnError },
        { Stat.FieldersChoices.Name, Stat.FieldersChoices },
        { Stat.CatchersInterference.Name, Stat.CatchersInterference },
        { Stat.GroundedIntoDoublePlay.Name, Stat.GroundedIntoDoublePlay },
        { Stat.GroundedIntoTriplePlay.Name, Stat.GroundedIntoTriplePlay },
        { Stat.AtBatsWithRunnersInScoringPosition.Name, Stat.AtBatsWithRunnersInScoringPosition },
        { Stat.HitsWithRunnersInScoringPosition.Name, Stat.HitsWithRunnersInScoringPosition },
        { Stat.Wins.Name, Stat.Wins },
        { Stat.Losses.Name, Stat.Losses },
        { Stat.Saves.Name, Stat.Saves },
        { Stat.ThirdInningsPitched.Name, Stat.ThirdInningsPitched },
        { Stat.BattersFaced.Name, Stat.BattersFaced },
        { Stat.Balls.Name, Stat.Balls },
        { Stat.Strikes.Name, Stat.Strikes },
        { Stat.Pitches.Name, Stat.Pitches },
        { Stat.EarnedRuns.Name, Stat.EarnedRuns },
        { Stat.IntentionalWalks.Name, Stat.IntentionalWalks },
        { Stat.Balks.Name, Stat.Balks },
        { Stat.WildPitches.Name, Stat.WildPitches },
        { Stat.GroundOuts.Name, Stat.GroundOuts },
        { Stat.AirOuts.Name, Stat.AirOuts },
        { Stat.FirstPitchStrikes.Name, Stat.FirstPitchStrikes },
        { Stat.FirstPitchBalls.Name, Stat.FirstPitchBalls },
        { Stat.Errors.Name, Stat.Errors },
        { Stat.ErrorsThrowing.Name, Stat.ErrorsThrowing },
        { Stat.ErrorsFielding.Name, Stat.ErrorsFielding },
        { Stat.Putouts.Name, Stat.Putouts },
        { Stat.Assists.Name, Stat.Assists },
        { Stat.StolenBaseAttempts.Name, Stat.StolenBaseAttempts },
        { Stat.DoublePlays.Name, Stat.DoublePlays },
        { Stat.TriplePlays.Name, Stat.TriplePlays },
        { Stat.PassedBalls.Name, Stat.PassedBalls },
        { Stat.PickoffFailed.Name, Stat.PickoffFailed },
        { Stat.PickoffSuccess.Name, Stat.PickoffSuccess },
    };
}
