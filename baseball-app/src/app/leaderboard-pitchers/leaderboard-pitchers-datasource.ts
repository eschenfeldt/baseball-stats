import { BaseballDataSource } from '../baseball-data-source'
import { LeaderboardPlayer } from '../contracts/leaderboard-player';
import { PagedApiParameters } from '../paged-api-parameters'

export interface PitcherLeaderboardParams extends PagedApiParameters {
    playerSearch?: string,
    playerId?: number,
    minInningsPitched?: number
    year?: number
}

export class LeaderboardPitchersDataSource extends BaseballDataSource<PitcherLeaderboardParams, LeaderboardPlayer> {

    protected override getParameters(): PitcherLeaderboardParams {
        return {};
    }

}