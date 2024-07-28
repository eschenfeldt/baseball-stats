import { BaseballDataSource } from '../baseball-data-source'
import { GameType } from '../game-type'
import { PagedApiParameters } from '../paged-api-parameters'
import { Park } from '../park'
import { Player } from '../player'
import { Team } from '../team'


export interface LeaderboardBatter {
    player: Player;
    year?: number;
    games: number;
    atBats: number;
    hits: number;
    battingAverage: number;
}

export enum BatterLeaderBoardOrder {
    Games,
    BattingAverage
}

export interface BatterLeaderboardParams extends PagedApiParameters {
    minPlateAppearances?: number,
    order: BatterLeaderBoardOrder,
    sortDescending: boolean
}

export class LeaderboardBattersDataSource extends BaseballDataSource<BatterLeaderboardParams, LeaderboardBatter> {

    protected override getParameters(): BatterLeaderboardParams {
        return {
            order: BatterLeaderBoardOrder.Games,
            sortDescending: true
        };
    }

}