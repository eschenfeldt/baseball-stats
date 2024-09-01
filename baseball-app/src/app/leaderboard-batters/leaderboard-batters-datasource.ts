import { BaseballDataSource } from '../baseball-data-source'
import { PagedApiParameters } from '../paged-api-parameters'
import { Player } from '../contracts/player'

export interface LeaderboardBatter {
    player: Player;
    year?: number;

    stats: {
        [statName: string]: number
    }
}

export interface BatterLeaderboardParams extends PagedApiParameters {
    minPlateAppearances?: number
}

export class LeaderboardBattersDataSource extends BaseballDataSource<BatterLeaderboardParams, LeaderboardBatter> {

    protected override getParameters(): BatterLeaderboardParams {
        return {
            minPlateAppearances: 30
        };
    }

}