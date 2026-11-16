import { Player } from "./player";

export interface GameBatter {
    player: Player;
    number: number;
    lineupPosition: number | null;
    appearanceOrder: number | null;

    stats: {
        [statName: string]: number
    }
}
