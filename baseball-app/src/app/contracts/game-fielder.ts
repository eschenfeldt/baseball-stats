import { Player } from "./player";

export interface GameFielder {
    player: Player;
    number: number;

    stats: {
        [statName: string]: number
    }
}
