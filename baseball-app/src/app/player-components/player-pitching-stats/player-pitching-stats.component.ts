import { Component, Input, OnChanges, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballApiService, ApiMethod } from '../../baseball-api.service';
import { BaseballApiFilter, BaseballFilterService } from '../../baseball-filter.service';
import { BaseballTableComponent } from '../../baseball-table-component';
import { LeaderboardPlayer } from '../../contracts/leaderboard-player';
import { StatDefCollection } from '../../contracts/stat-def';
import { PlayerGamesParameters } from '../player-games/player-games-datasource';
import { LeaderboardPitchersDataSource, PitcherLeaderboardParams } from '../../leaderboard-pitchers/leaderboard-pitchers-datasource';
import { LeaderboardPitchersComponent } from '../../leaderboard-pitchers/leaderboard-pitchers.component';
import { AsyncPipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { StatPipe } from '../../stat.pipe';
import { TypeSafeMatCellDef } from '../../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../../type-safe-mat-row-def.directive';
import { Utils } from '../../utils';

@Component({
    selector: 'app-player-pitching-stats',
    standalone: true,
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatPaginatorModule,
        MatSortModule,
        AsyncPipe,
        StatPipe
    ],
    templateUrl: './player-pitching-stats.component.html',
    styleUrl: './player-pitching-stats.component.scss'
})
export class PlayerPitchingStatsComponent extends BaseballTableComponent<PitcherLeaderboardParams, LeaderboardPlayer> implements OnInit, OnChanges {

    @Input({ required: true })
    playerId!: number;
    @Input()
    gamesIdentifier?: string;

    @ViewChild(MatPaginator)
    protected paginator!: MatPaginator;
    @ViewChild(MatSort)
    protected sort!: MatSort;

    dataSource: LeaderboardPitchersDataSource;
    displayedColumns: string[] = [
        'year'
    ];
    protected override get defaultFilters(): BaseballApiFilter {
        return {
            playerId: this.playerId,
            sort: 'year'
        };
    }
    override defaultPageSize: number = 5;

    stats: StatDefCollection = {};
    get statNames(): string[] {
        return Object.keys(this.stats).filter(n => n !== 'ThirdInningsPitched');
    }

    constructor(
        api: BaseballApiService,
        private filterService: BaseballFilterService
    ) {
        super();
        this.dataSource = new LeaderboardPitchersDataSource(
            LeaderboardPitchersComponent.endpoint,
            ApiMethod.POST,
            api,
            filterService,
            false,
            this.defaultFilters
        );
    }

    public override ngOnInit(): void {
        this.initialize()
    }

    public ngOnChanges(): void {
        this.initialize()
    }

    private initialize(): void {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'playerId', this.playerId);
        this.dataSource.stats$.subscribe(stats => {
            this.stats = stats;
            this.displayedColumns = [
                'year',
                'inningsPitched',
                ...this.statNames
            ]
        })
    }

    public setYear(year: number | undefined): void {
        if (this.gamesIdentifier) {
            this.filterService.setFilterValue<PlayerGamesParameters>(this.gamesIdentifier, 'year', year);
        }
    }


    fullInningsPitched(pitcher: LeaderboardPlayer): string {
        return Utils.fullInningsPitched(pitcher.stats);
    }
    partialInningsPitched(pitcher: LeaderboardPlayer): string {
        return Utils.partialInningsPitched(pitcher.stats);
    }
}
