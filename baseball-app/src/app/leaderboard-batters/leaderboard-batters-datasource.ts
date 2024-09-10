import { BaseballDataSource } from '../baseball-data-source'
import { LeaderboardPlayer } from '../contracts/leaderboard-player';
import { PagedApiParameters } from '../paged-api-parameters'

export interface BatterLeaderboardParams extends PagedApiParameters {
    playerSearch?: string,
    minPlateAppearances?: number
}

export class LeaderboardBattersDataSource extends BaseballDataSource<BatterLeaderboardParams, LeaderboardPlayer> {

    protected override getParameters(): BatterLeaderboardParams {
        return {};
    }

}