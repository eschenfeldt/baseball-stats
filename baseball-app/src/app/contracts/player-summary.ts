import { Player } from './player';
import { SummaryStat } from './summary-stat';

export interface PlayerSummary {
    info: Player;
    summaryStats: SummaryStat[]
}
