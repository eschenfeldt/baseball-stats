import { BoxScoreDetail } from "./box-score-detail";
import { GameSummary } from "./game-summary";

export interface GameDetail extends GameSummary {
    awayBoxScore: BoxScoreDetail;
    homeBoxScore: BoxScoreDetail;
}
