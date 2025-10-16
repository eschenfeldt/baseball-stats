import { GameType } from './game-type';
import { Team } from './team';

export interface GameMetadata {
    home: Team;
    away: Team;
    gameType: GameType;
    scheduledStart: Date | null;
    actualStart: Date | null;
    end: Date | null;
}
