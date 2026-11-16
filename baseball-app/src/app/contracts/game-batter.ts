import { Player } from "./player";

export interface GameBatter {
    player: Player;
    number: number;
    lineupPosition: number;
    appearanceOrder: number;

    stats: {
        [statName: string]: number
    }
}
