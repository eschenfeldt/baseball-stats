import { GameBatter } from './game-batter';
import { GameFielder } from './game-fielder';
import { GamePitcher } from './game-pitcher';
import { GameSummary } from './game-summary';

export interface PlayerGame {
    game: GameSummary;
    batter?: GameBatter;
    pitcher?: GamePitcher;
    fielder?: GameFielder;
}
