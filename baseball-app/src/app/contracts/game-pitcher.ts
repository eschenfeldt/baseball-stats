import { Player } from "./player";

export interface GamePitcher {
    player: Player;
    number: number;

    stats: {
        ThirdInningsPitched: number,
        [statName: string]: number
    }
}
