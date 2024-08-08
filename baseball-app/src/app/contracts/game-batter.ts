import { Player } from "./player";

export interface GameBatter {
    player: Player;

    games: number;
    atBats: number;
    hits: number;
}
