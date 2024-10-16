import { PagedResult } from './paged-result';
import { PlayerGame } from './player-game';
import { StatDefCollection } from './stat-def';

export interface PlayerGameResults extends PagedResult<PlayerGame> {
    pitchingStats: StatDefCollection;
    battingStats: StatDefCollection;
}
