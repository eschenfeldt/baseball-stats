import { Player } from "./player";

export interface GamePitcher {
    player: Player;
    number: number;
    appearanceOrder: number | null;

    stats: {
        ThirdInningsPitched: number,
        [statName: string]: number
    }
}
