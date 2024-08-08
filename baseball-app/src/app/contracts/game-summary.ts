import { GameType } from "./game-type";
import { Park } from "./park";
import { Player } from "./player";
import { Team } from "./team";

export interface GameSummary {
    id: number,
    externalId: number,
    name: string,
    date: Date,
    gameType?: GameType,
    home: Team,
    homeTeamName: string,
    away: Team,
    awayTeamName: string,
    scheduledTime?: string,
    startTime?: string,
    endTime?: string,
    location?: Park,
    homeScore: number,
    awayScore: number,
    winningTeam?: Team,
    losingTeam?: Team,
    winningPitcher?: Player,
    losingPitcher?: Player,
    savingPitcher?: Player
}
