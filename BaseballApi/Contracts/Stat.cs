using System;

namespace BaseballApi.Contracts;

public struct Stat
{
    public string Name { get; set; }
    public string? ShortName { get; set; }
    public string? LongName { get; set; }
    public StatFormat Format { get; set; }

    public static readonly Stat Games = new()
    {
        Name = "Games",
        ShortName = "G",
        Format = StatFormat.Integer
    };
    public static readonly Stat Parks = new()
    {
        Name = "Parks",
        ShortName = null,
        Format = StatFormat.Integer
    };
    public static readonly Stat PlateAppearances = new()
    {
        Name = "PlateAppearances",
        ShortName = "PA",
        Format = StatFormat.Integer
    };
    public static readonly Stat AtBats = new()
    {
        Name = "AtBats",
        ShortName = "AB",
        Format = StatFormat.Integer
    };
    public static readonly Stat Runs = new()
    {
        Name = "Runs",
        ShortName = "R",
        Format = StatFormat.Integer
    };
    public static readonly Stat Hits = new()
    {
        Name = "Hits",
        ShortName = "H",
        Format = StatFormat.Integer
    };
    public static readonly Stat BuntSingles = new()
    {
        Name = "BuntSingles",
        ShortName = "Bunt 1B",
        Format = StatFormat.Integer
    };
    public static readonly Stat Singles = new()
    {
        Name = "Singles",
        ShortName = "1B",
        Format = StatFormat.Integer
    };
    public static readonly Stat Doubles = new()
    {
        Name = "Doubles",
        ShortName = "2B",
        Format = StatFormat.Integer
    };
    public static readonly Stat Triples = new()
    {
        Name = "Triples",
        ShortName = "3B",
        Format = StatFormat.Integer
    };
    public static readonly Stat Homeruns = new()
    {
        Name = "Homeruns",
        ShortName = "HR",
        Format = StatFormat.Integer
    };
    public static readonly Stat RunsBattedIn = new()
    {
        Name = "RunsBattedIn",
        ShortName = "RBI",
        Format = StatFormat.Integer
    };
    public static readonly Stat Walks = new()
    {
        Name = "Walks",
        ShortName = "BB",
        Format = StatFormat.Integer
    };
    public static readonly Stat Strikeouts = new()
    {
        Name = "Strikeouts",
        ShortName = "K",
        Format = StatFormat.Integer
    };
    public static readonly Stat StrikeoutsCalled = new()
    {
        Name = "StrikeoutsCalled",
        ShortName = "Kc",
        Format = StatFormat.Integer
    };
    public static readonly Stat StrikeoutsSwinging = new()
    {
        Name = "StrikeoutsSwinging",
        ShortName = "Ks",
        Format = StatFormat.Integer
    };
    public static readonly Stat HitByPitch = new()
    {
        Name = "HitByPitch",
        ShortName = "HBP",
        Format = StatFormat.Integer
    };
    public static readonly Stat StolenBases = new()
    {
        Name = "StolenBases",
        ShortName = "SB",
        Format = StatFormat.Integer
    };
    public static readonly Stat CaughtStealing = new()
    {
        Name = "CaughtStealing",
        ShortName = "CS",
        Format = StatFormat.Integer
    };
    public static readonly Stat SacrificeBunts = new()
    {
        Name = "SacrificeBunts",
        ShortName = "Sac Bunt",
        Format = StatFormat.Integer
    };
    public static readonly Stat SacrificeFlies = new()
    {
        Name = "SacrificeFlies",
        ShortName = "SF",
        Format = StatFormat.Integer
    };
    public static readonly Stat Sacrifices = new()
    {
        Name = "Sacrifices",
        ShortName = "SAC",
        Format = StatFormat.Integer
    };
    public static readonly Stat ReachedOnError = new()
    {
        Name = "ReachedOnError",
        ShortName = "E",
        Format = StatFormat.Integer
    };
    public static readonly Stat FieldersChoices = new()
    {
        Name = "FieldersChoices",
        ShortName = "FC",
        Format = StatFormat.Integer
    };
    public static readonly Stat CatchersInterference = new()
    {
        Name = "CatchersInterference",
        ShortName = "CI",
        Format = StatFormat.Integer
    };
    public static readonly Stat GroundedIntoDoublePlay = new()
    {
        Name = "GroundedIntoDoublePlay",
        ShortName = "GIDP",
        Format = StatFormat.Integer
    };
    public static readonly Stat GroundedIntoTriplePlay = new()
    {
        Name = "GroundedIntoTriplePlay",
        ShortName = "GITP",
        Format = StatFormat.Integer
    };
    public static readonly Stat AtBatsWithRunnersInScoringPosition = new()
    {
        Name = "AtBatsWithRunnersInScoringPosition",
        ShortName = "AB RISP",
        Format = StatFormat.Integer
    };
    public static readonly Stat HitsWithRunnersInScoringPosition = new()
    {
        Name = "HitsWithRunnersInScoringPosition",
        ShortName = "H RISP",
        Format = StatFormat.Integer
    };

    public static readonly Stat Wins = new()
    {
        Name = "Wins",
        ShortName = "W",
        Format = StatFormat.Integer
    };
    public static readonly Stat Losses = new()
    {
        Name = "Losses",
        ShortName = "L",
        Format = StatFormat.Integer
    };
    public static readonly Stat Saves = new()
    {
        Name = "Saves",
        ShortName = "Sv",
        Format = StatFormat.Integer
    };
    public static readonly Stat ThirdInningsPitched = new()
    {
        Name = "ThirdInningsPitched",
        Format = StatFormat.Integer
    };
    public static readonly Stat BattersFaced = new()
    {
        Name = "BattersFaced",
        ShortName = "BF",
        Format = StatFormat.Integer
    };
    public static readonly Stat Balls = new()
    {
        Name = "Balls",
        ShortName = "B",
        Format = StatFormat.Integer
    };
    public static readonly Stat Strikes = new()
    {
        Name = "Strikes",
        ShortName = "S",
        Format = StatFormat.Integer
    };
    public static readonly Stat Pitches = new()
    {
        Name = "Pitches",
        ShortName = "P",
        Format = StatFormat.Integer
    };
    public static readonly Stat EarnedRuns = new()
    {
        Name = "EarnedRuns",
        ShortName = "ER",
        Format = StatFormat.Integer
    };
    public static readonly Stat IntentionalWalks = new()
    {
        Name = "IntentionalWalks",
        ShortName = "IBB",
        Format = StatFormat.Integer
    };
    public static readonly Stat Balks = new()
    {
        Name = "Balks",
        ShortName = "Bk",
        Format = StatFormat.Integer
    };
    public static readonly Stat WildPitches = new()
    {
        Name = "WildPitches",
        ShortName = "WP",
        Format = StatFormat.Integer
    };
    public static readonly Stat GroundOuts = new()
    {
        Name = "GroundOuts",
        ShortName = "GO",
        Format = StatFormat.Integer
    };
    public static readonly Stat AirOuts = new()
    {
        Name = "AirOuts",
        ShortName = "AO",
        Format = StatFormat.Integer
    };
    public static readonly Stat FirstPitchStrikes = new()
    {
        Name = "FirstPitchStrikes",
        ShortName = "FPS",
        Format = StatFormat.Integer
    };
    public static readonly Stat FirstPitchBalls = new()
    {
        Name = "FirstPitchBalls",
        ShortName = "FPB",
        Format = StatFormat.Integer
    };
    public static readonly Stat Errors = new()
    {
        Name = "Errors",
        ShortName = "E",
        Format = StatFormat.Integer
    };
    public static readonly Stat ErrorsThrowing = new()
    {
        Name = "ErrorsThrowing",
        ShortName = "TE",
        Format = StatFormat.Integer
    };
    public static readonly Stat ErrorsFielding = new()
    {
        Name = "ErrorsFielding",
        ShortName = "FE",
        Format = StatFormat.Integer
    };
    public static readonly Stat Putouts = new()
    {
        Name = "Putouts",
        ShortName = "PO",
        Format = StatFormat.Integer
    };
    public static readonly Stat Assists = new()
    {
        Name = "Assists",
        ShortName = "A",
        Format = StatFormat.Integer
    };
    public static readonly Stat StolenBaseAttempts = new()
    {
        Name = "StolenBaseAttempts",
        ShortName = "SBA",
        Format = StatFormat.Integer
    };
    public static readonly Stat DoublePlays = new()
    {
        Name = "DoublePlays",
        ShortName = "DP",
        Format = StatFormat.Integer
    };
    public static readonly Stat TriplePlays = new()
    {
        Name = "TriplePlays",
        ShortName = "TP",
        Format = StatFormat.Integer
    };
    public static readonly Stat PassedBalls = new()
    {
        Name = "PassedBalls",
        ShortName = "PB",
        Format = StatFormat.Integer
    };
    public static readonly Stat PickoffFailed = new()
    {
        Name = "PickoffFailed",
        ShortName = "PO F",
        Format = StatFormat.Integer
    };
    public static readonly Stat PickoffSuccess = new()
    {
        Name = "PickoffSuccess",
        ShortName = "PO S",
        Format = StatFormat.Integer
    };
    public static readonly Stat BattingAverage = new()
    {
        Name = "BattingAverage",
        ShortName = "AVG",
        Format = StatFormat.Decimal3
    };
    public static readonly Stat OnBasePercentage = new()
    {
        Name = "OnBasePercentage",
        ShortName = "OBP",
        Format = StatFormat.Decimal3
    };
    public static readonly Stat SluggingPercentage = new()
    {
        Name = "SluggingPercentage",
        ShortName = "SLG",
        Format = StatFormat.Decimal3
    };
    public static readonly Stat OnBasePlusSlugging = new()
    {
        Name = "OnBasePlusSlugging",
        ShortName = "OPS",
        Format = StatFormat.Decimal3
    };
    public static readonly Stat WeightedOnBaseAverage = new()
    {
        Name = "WeightedOnBaseAverage",
        ShortName = "wOBA",
        Format = StatFormat.Decimal3
    };
    public static readonly Stat EarnedRunAverage = new()
    {
        Name = "EarnedRunAverage",
        ShortName = "ERA",
        Format = StatFormat.Decimal2
    };
    public static readonly Stat FieldingIndependentPitching = new()
    {
        Name = "FieldingIndependentPitching",
        ShortName = "FIP",
        Format = StatFormat.Decimal2
    };
    public static readonly Stat HomerunsAllowed = new()
    {
        Name = "HomerunsAllowed",
        ShortName = "HR",
        Format = StatFormat.Integer
    };
    public static readonly Stat StrikeoutRate = new()
    {
        Name = "StrikeoutRate",
        ShortName = "K%",
        Format = StatFormat.Percentage1
    };
    public static readonly Stat WalkRate = new()
    {
        Name = "WalkRate",
        ShortName = "BB%",
        Format = StatFormat.Percentage1
    };
}
