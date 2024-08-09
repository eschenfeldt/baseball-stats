import { Player } from "./player";

export interface GameFielder {
    player: Player;
    number: number;

    games: number;
    errors: number;
    errorsThrowing: number;
    errorsFielding: number;
    putouts: number;
    assists: number;
    stolenBaseAttempts: number;
    caughtStealing: number;
    doublePlays: number;
    triplePlays: number;
    passedBalls: number;
    pickoffFailed: number;
    pickoffSuccess: number;
}
