using System;

namespace BaseballApi.Contracts;

public struct Stat
{
    public string Name { get; set; }
    public string? ShortName { get; set; }
    public string? LongName { get; set; }
    public IStatFormat Format { get; set; }

    public static readonly Stat Games = new()
    {
        Name = "Games",
        ShortName = "G",
        Format = Integer.Singleton
    };
    public static readonly Stat PlateAppearances = new()
    {
        Name = "PlateAppearances",
        ShortName = "PA",
        Format = Integer.Singleton
    };
    public static readonly Stat AtBats = new()
    {
        Name = "AtBats",
        ShortName = "AB",
        Format = Integer.Singleton
    };
    public static readonly Stat Runs = new()
    {
        Name = "Runs",
        ShortName = "R",
        Format = Integer.Singleton
    };
    public static readonly Stat Hits = new()
    {
        Name = "Hits",
        ShortName = "H",
        Format = Integer.Singleton
    };
    public static readonly Stat BuntSingles = new()
    {
        Name = "BuntSingles",
        ShortName = "Bunt 1B",
        Format = Integer.Singleton
    };
    public static readonly Stat Singles = new()
    {
        Name = "Singles",
        ShortName = "1B",
        Format = Integer.Singleton
    };
    public static readonly Stat Doubles = new()
    {
        Name = "Doubles",
        ShortName = "2B",
        Format = Integer.Singleton
    };
    public static readonly Stat Triples = new()
    {
        Name = "Triples",
        ShortName = "3B",
        Format = Integer.Singleton
    };
    public static readonly Stat Homeruns = new()
    {
        Name = "Homeruns",
        ShortName = "HR",
        Format = Integer.Singleton
    };
    public static readonly Stat RunsBattedIn = new()
    {
        Name = "RunsBattedIn",
        ShortName = "RBI",
        Format = Integer.Singleton
    };
    public static readonly Stat Walks = new()
    {
        Name = "Walks",
        ShortName = "BB",
        Format = Integer.Singleton
    };
    public static readonly Stat Strikeouts = new()
    {
        Name = "Strikeouts",
        ShortName = "K",
        Format = Integer.Singleton
    };
    public static readonly Stat StrikeoutsCalled = new()
    {
        Name = "StrikeoutsCalled",
        ShortName = "Kc",
        Format = Integer.Singleton
    };
    public static readonly Stat StrikeoutsSwinging = new()
    {
        Name = "StrikeoutsSwinging",
        ShortName = "Ks",
        Format = Integer.Singleton
    };
    public static readonly Stat HitByPitch = new()
    {
        Name = "HitByPitch",
        ShortName = "HBP",
        Format = Integer.Singleton
    };
    public static readonly Stat StolenBases = new()
    {
        Name = "StolenBases",
        ShortName = "SB",
        Format = Integer.Singleton
    };
    public static readonly Stat CaughtStealing = new()
    {
        Name = "CaughtStealing",
        ShortName = "CS",
        Format = Integer.Singleton
    };
    public static readonly Stat SacrificeBunts = new()
    {
        Name = "SacrificeBunts",
        ShortName = "Sac Bunt",
        Format = Integer.Singleton
    };
    public static readonly Stat SacrificeFlies = new()
    {
        Name = "SacrificeFlies",
        ShortName = "SF",
        Format = Integer.Singleton
    };
    public static readonly Stat Sacrifices = new()
    {
        Name = "Sacrifices",
        ShortName = "SAC",
        Format = Integer.Singleton
    };
    public static readonly Stat ReachedOnError = new()
    {
        Name = "ReachedOnError",
        ShortName = "E",
        Format = Integer.Singleton
    };
    public static readonly Stat FieldersChoices = new()
    {
        Name = "FieldersChoices",
        ShortName = "FC",
        Format = Integer.Singleton
    };
    public static readonly Stat CatchersInterference = new()
    {
        Name = "CatchersInterference",
        ShortName = "CI",
        Format = Integer.Singleton
    };
    public static readonly Stat GroundedIntoDoublePlay = new()
    {
        Name = "GroundedIntoDoublePlay",
        ShortName = "GIDP",
        Format = Integer.Singleton
    };
    public static readonly Stat GroundedIntoTriplePlay = new()
    {
        Name = "GroundedIntoTriplePlay",
        ShortName = "GITP",
        Format = Integer.Singleton
    };
    public static readonly Stat AtBatsWithRunnersInScoringPosition = new()
    {
        Name = "AtBatsWithRunnersInScoringPosition",
        ShortName = "AB RISP",
        Format = Integer.Singleton
    };
    public static readonly Stat HitsWithRunnersInScoringPosition = new()
    {
        Name = "HitsWithRunnersInScoringPosition",
        ShortName = "H RISP",
        Format = Integer.Singleton
    };

    public static readonly Stat Wins = new()
    {
        Name = "Wins",
        ShortName = "W",
        Format = Integer.Singleton
    };
    public static readonly Stat Losses = new()
    {
        Name = "Losses",
        ShortName = "L",
        Format = Integer.Singleton
    };
    public static readonly Stat Saves = new()
    {
        Name = "Saves",
        ShortName = "Sv",
        Format = Integer.Singleton
    };
    public static readonly Stat ThirdInningsPitched = new()
    {
        Name = "ThirdInningsPitched",
        Format = Integer.Singleton
    };
    public static readonly Stat BattersFaced = new()
    {
        Name = "BattersFaced",
        ShortName = "BF",
        Format = Integer.Singleton
    };
    public static readonly Stat Balls = new()
    {
        Name = "Balls",
        ShortName = "B",
        Format = Integer.Singleton
    };
    public static readonly Stat Strikes = new()
    {
        Name = "Strikes",
        ShortName = "S",
        Format = Integer.Singleton
    };
    public static readonly Stat Pitches = new()
    {
        Name = "Pitches",
        ShortName = "P",
        Format = Integer.Singleton
    };
    public static readonly Stat EarnedRuns = new()
    {
        Name = "EarnedRuns",
        ShortName = "ER",
        Format = Integer.Singleton
    };
    public static readonly Stat IntentionalWalks = new()
    {
        Name = "IntentionalWalks",
        ShortName = "IBB",
        Format = Integer.Singleton
    };
    public static readonly Stat Balks = new()
    {
        Name = "Balks",
        ShortName = "Bk",
        Format = Integer.Singleton
    };
    public static readonly Stat WildPitches = new()
    {
        Name = "WildPitches",
        ShortName = "WP",
        Format = Integer.Singleton
    };
    public static readonly Stat GroundOuts = new()
    {
        Name = "GroundOuts",
        ShortName = "GO",
        Format = Integer.Singleton
    };
    public static readonly Stat AirOuts = new()
    {
        Name = "AirOuts",
        ShortName = "AO",
        Format = Integer.Singleton
    };
    public static readonly Stat FirstPitchStrikes = new()
    {
        Name = "FirstPitchStrikes",
        ShortName = "FPS",
        Format = Integer.Singleton
    };
    public static readonly Stat FirstPitchBalls = new()
    {
        Name = "FirstPitchBalls",
        ShortName = "FPB",
        Format = Integer.Singleton
    };
    public static readonly Stat Errors = new()
    {
        Name = "Errors",
        ShortName = "E",
        Format = Integer.Singleton
    };
    public static readonly Stat ErrorsThrowing = new()
    {
        Name = "ErrorsThrowing",
        ShortName = "TE",
        Format = Integer.Singleton
    };
    public static readonly Stat ErrorsFielding = new()
    {
        Name = "ErrorsFielding",
        ShortName = "FE",
        Format = Integer.Singleton
    };
    public static readonly Stat Putouts = new()
    {
        Name = "Putouts",
        ShortName = "PO",
        Format = Integer.Singleton
    };
    public static readonly Stat Assists = new()
    {
        Name = "Assists",
        ShortName = "A",
        Format = Integer.Singleton
    };
    public static readonly Stat StolenBaseAttempts = new()
    {
        Name = "StolenBaseAttempts",
        ShortName = "SBA",
        Format = Integer.Singleton
    };
    public static readonly Stat DoublePlays = new()
    {
        Name = "DoublePlays",
        ShortName = "DP",
        Format = Integer.Singleton
    };
    public static readonly Stat TriplePlays = new()
    {
        Name = "TriplePlays",
        ShortName = "TP",
        Format = Integer.Singleton
    };
    public static readonly Stat PassedBalls = new()
    {
        Name = "PassedBalls",
        ShortName = "PB",
        Format = Integer.Singleton
    };
    public static readonly Stat PickoffFailed = new()
    {
        Name = "PickoffFailed",
        ShortName = "PO F",
        Format = Integer.Singleton
    };
    public static readonly Stat PickoffSuccess = new()
    {
        Name = "PickoffSuccess",
        ShortName = "PO S",
        Format = Integer.Singleton
    };
    public static readonly Stat BattingAverage = new()
    {
        Name = "BattingAverage",
        ShortName = "AVG",
        Format = new Decimal(3)
    };
    public static readonly Stat OnBasePercentage = new()
    {
        Name = "OnBasePercentage",
        ShortName = "OBP",
        Format = new Decimal(3)
    };
    public static readonly Stat WeightedOnBaseAverage = new()
    {
        Name = "WeightedOnBaseAverage",
        ShortName = "wOBA",
        Format = new Decimal(3)
    };
    public static readonly Stat EarnedRunAverage = new()
    {
        Name = "EarnedRunAverage",
        ShortName = "ERA",
        Format = new Decimal(2)
    };
    public static readonly Stat FieldingIndependentPitching = new()
    {
        Name = "FieldingIndependentPitching",
        ShortName = "FIP",
        Format = new Decimal(2)
    };
}
