import { BaseballDataSource } from '../baseball-data-source'
import { GameType } from '../game-type'
import { PagedApiParameters } from '../paged-api-parameters'
import { Park } from '../park'
import { Player } from '../player'
import { Team } from '../team'


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
    scheduledTime?: Date,
    startTime?: Date,
    endTime?: Date,
    location?: Park,
    homeScore: number,
    awayScore: number,
    winningTeam?: Team,
    losingTeam?: Team,
    winningPitcher?: Player,
    losingPitcher?: Player,
    savingPitcher?: Player
}

export interface GamesListParams extends PagedApiParameters {

}

export class GamesDataSource extends BaseballDataSource<GamesListParams, GameSummary> {

    protected override getParameters(): GamesListParams {
        return {};
    }

}