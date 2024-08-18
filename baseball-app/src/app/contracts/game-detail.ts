import { BoxScoreDetail } from "./box-score-detail";
import { GameSummary } from "./game-summary";
import { ScorecardDetail } from "./scorecard-detail";

export interface GameDetail extends GameSummary {
    scorecard?: ScorecardDetail
    awayBoxScore: BoxScoreDetail;
    homeBoxScore: BoxScoreDetail;
}
