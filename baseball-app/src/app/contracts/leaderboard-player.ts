import { Player } from './player';

export interface LeaderboardPlayer {
    player: Player;
    year?: number;

    stats: {
        [statName: string]: number
    }
}
