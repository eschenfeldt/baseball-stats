import { ApiMethod, BaseballApiService } from '../baseball-api.service'
import { BaseballDataSource } from '../baseball-data-source'
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service'
import { GameType } from '../contracts/game-type'
import { PagedApiParameters } from '../paged-api-parameters'
import { Park } from '../contracts/park'
import { Player } from '../contracts/player'
import { Team } from '../contracts/team'


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

export interface GamesListParams extends PagedApiParameters {
    teamId?: number;
    year?: number;
}

export class GamesDataSource extends BaseballDataSource<GamesListParams, GameSummary> {

    public constructor(
        api: BaseballApiService,
        filterService: BaseballFilterService,
        defaultFilters?: BaseballApiFilter
    ) {
        super('games', ApiMethod.GET, api, filterService, false, defaultFilters);
    }

    protected override getParameters(): GamesListParams {
        return {};
    }

}