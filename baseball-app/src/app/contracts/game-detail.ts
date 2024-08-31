import { BoxScoreDetail } from './box-score-detail';
import { GameSummary } from './game-summary';
import { ScorecardDetail } from './scorecard-detail';
import { StatDefCollection } from './stat-def';

export interface GameDetail extends GameSummary {
    scorecard?: ScorecardDetail;
    hasMedia: boolean;
    stats: StatDefCollection;
    awayBoxScore: BoxScoreDetail;
    homeBoxScore: BoxScoreDetail;
}
