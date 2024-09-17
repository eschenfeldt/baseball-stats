import { ApiMethod, BaseballApiService } from '../../baseball-api.service';
import { BaseballDataSource } from '../../baseball-data-source';
import { BaseballFilterService } from '../../baseball-filter.service';
import { PlayerGame } from '../../contracts/player-game';
import { PagedApiParameters } from '../../paged-api-parameters';


export interface PlayerGamesParameters extends PagedApiParameters {
    playerId?: number;
    year?: number;
}

export class PlayerGamesDataSource extends BaseballDataSource<PlayerGamesParameters, PlayerGame> {

    public constructor(
        api: BaseballApiService,
        filterService: BaseballFilterService
    ) {
        super(
            `player/games`,
            ApiMethod.GET,
            api,
            filterService,
            false,
            {}
        )
    }

    protected override getParameters(): PlayerGamesParameters {
        return {};
    }

}