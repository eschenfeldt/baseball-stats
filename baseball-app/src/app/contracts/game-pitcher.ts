import { Player } from "./player";

export interface GamePitcher {
    player: Player;
    number: number;
    appearanceOrder: number;

    stats: {
        ThirdInningsPitched: number,
        [statName: string]: number
    }
}
