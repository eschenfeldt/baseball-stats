import { BaseballDataSource } from '../baseball-data-source'
import { LeaderboardPlayer } from '../contracts/leaderboard-player';
import { PagedApiParameters } from '../paged-api-parameters'

export interface BatterLeaderboardParams extends PagedApiParameters {
    playerId?: number
    playerSearch?: string
    minPlateAppearances?: number
    year?: number
    teamId?: number
    parkId?: number
}

export class LeaderboardBattersDataSource extends BaseballDataSource<BatterLeaderboardParams, LeaderboardPlayer> {

    protected override getParameters(): BatterLeaderboardParams {
        return {};
    }

}