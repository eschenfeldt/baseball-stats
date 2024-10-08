import { BehaviorSubject } from 'rxjs';
import { ApiMethod, BaseballApiService } from '../../baseball-api.service';
import { BaseballDataSource } from '../../baseball-data-source';
import { BaseballFilterService } from '../../baseball-filter.service';
import { PlayerGame } from '../../contracts/player-game';
import { StatDefCollection } from '../../contracts/stat-def';
import { PagedApiParameters } from '../../paged-api-parameters';
import { PagedResult } from '../../contracts/paged-result';
import { PlayerGameResults } from '../../contracts/player-game-results';


export interface PlayerGamesParameters extends PagedApiParameters {
    playerId?: number;
    year?: number;
}

export class PlayerGamesDataSource extends BaseballDataSource<PlayerGamesParameters, PlayerGame> {

    private pitchingStatsSubject = new BehaviorSubject<StatDefCollection>({});
    private battingStatsSubject = new BehaviorSubject<StatDefCollection>({});

    public pitchingStats$ = this.pitchingStatsSubject.asObservable();
    public battingStats$ = this.battingStatsSubject.asObservable();

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

    protected override postProcess(data: PagedResult<PlayerGame>): void {
        const results = data as PlayerGameResults;
        if (results) {
            this.battingStatsSubject.next(results.battingStats);
            this.pitchingStatsSubject.next(results.pitchingStats);
        }
    }
}