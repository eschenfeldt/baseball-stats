import { Player } from "./player";

export interface GamePitcher {
    player: Player;

    number: number;
    games: number;
    wins: number;
    losses: number;
    saves: number;
    thirdInningsPitched: number;
    battersFaced: number;
    balls: number;
    strikes: number;
    pitches: number;
    runs: number;
    earnedRuns: number;
    hits: number;
    walks: number;
    intentionalWalks: number;
    strikeouts: number;
    strikeoutsCalled: number;
    strikeoutsSwinging: number;
    hitByPitch: number;
    balks: number;
    wildPitches: number;
    homeruns: number;
    groundOuts: number;
    airOuts: number;
    firstPitchStrikes: number;
    firstPitchBalls: number;
}
