import { PagedResult } from './paged-result';
import { StatDefCollection } from './stat-def';

export interface Leaderboard<T> extends PagedResult<T> {
    stats: StatDefCollection;
}
