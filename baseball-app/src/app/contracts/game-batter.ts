import { Player } from "./player";

export interface GameBatter {
    player: Player;
    number: number;

    stats: {
        [statName: string]: number
    }
}
