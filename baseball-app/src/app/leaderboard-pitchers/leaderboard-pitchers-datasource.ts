import { BaseballDataSource } from '../baseball-data-source'
import { GameType } from '../contracts/game-type';
import { LeaderboardPlayer } from '../contracts/leaderboard-player';
import { PagedApiParameters } from '../paged-api-parameters'

export interface PitcherLeaderboardParams extends PagedApiParameters {
    playerSearch?: string
    playerId?: number
    minInningsPitched?: number
    year?: number
    teamId?: number
    parkId?: number
    gameType?: GameType
}

export class LeaderboardPitchersDataSource extends BaseballDataSource<PitcherLeaderboardParams, LeaderboardPlayer> {

    protected override getParameters(): PitcherLeaderboardParams {
        return {};
    }

}