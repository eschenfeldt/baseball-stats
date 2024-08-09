import { Player } from "./player";

export interface GameBatter {
    player: Player;
    number: number;

    games: number;
    plateAppearances: number;
    atBats: number;
    runs: number;
    hits: number;
    buntSingles: number;
    singles: number;
    doubles: number;
    triples: number;
    homeruns: number;
    runsBattedIn: number;
    walks: number;
    strikeouts: number;
    strikeoutsCalled: number;
    strikeoutsSwinging: number;
    hitByPitch: number;
    stolenBases: number;
    caughtStealing: number;
    sacrificeBunts: number;
    sacrificeFlies: number;
    sacrifices: number;
    reachedOnError: number;
    fieldersChoices: number;
    catchersInterference: number;
    groundedIntoDoublePlay: number;
    groundedIntoTriplePlay: number;
    atBatsWithRunnersInScoringPosition: number;
    hitsWithRunnersInScoringPosition: number;
}
