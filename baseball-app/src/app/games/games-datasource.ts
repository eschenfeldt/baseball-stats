import { ApiMethod, BaseballApiService } from '../baseball-api.service'
import { BaseballDataSource } from '../baseball-data-source'
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service'
import { GameSummary } from '../contracts/game-summary';
import { PagedApiParameters } from '../paged-api-parameters'

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