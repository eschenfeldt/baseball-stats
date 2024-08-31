import { GameBatter } from "./game-batter";
import { GameFielder } from "./game-fielder";
import { GamePitcher } from "./game-pitcher";

export interface BoxScoreDetail {
    batters: GameBatter[];
    pitchers: GamePitcher[];
    fielders: GameFielder[];
}
