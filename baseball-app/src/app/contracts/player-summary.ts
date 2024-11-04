import { Player } from './player';
import { RemoteFileDetail } from './remote-file-detail';
import { SummaryStat } from './summary-stat';

export interface PlayerSummary {
    info: Player;
    photo: RemoteFileDetail;
    summaryStats: SummaryStat[];
}
