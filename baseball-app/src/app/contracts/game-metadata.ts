import { Team } from './team';

export interface GameMetadata {
    home: Team;
    away: Team;
    scheduledStart: Date | null;
    actualStart: Date | null;
    end: Date | null;
}
