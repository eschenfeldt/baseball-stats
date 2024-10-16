import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { BaseballTableComponent } from '../../baseball-table-component';
import { BatterLeaderboardParams, LeaderboardBattersDataSource } from '../../leaderboard-batters/leaderboard-batters-datasource';
import { LeaderboardPlayer } from '../../contracts/leaderboard-player';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballApiFilter, BaseballFilterService } from '../../baseball-filter.service';
import { ApiMethod, BaseballApiService } from '../../baseball-api.service';
import { LeaderboardBattersComponent } from '../../leaderboard-batters/leaderboard-batters.component';
import { StatDefCollection } from '../../contracts/stat-def';
import { AsyncPipe, CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../../type-safe-mat-row-def.directive';
import { StatPipe } from '../../stat.pipe';
import { PlayerGamesParameters } from '../player-games/player-games-datasource';

@Component({
    selector: 'app-player-batting-stats',
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
    templateUrl: './player-batting-stats.component.html',
    styleUrl: './player-batting-stats.component.scss'
})
export class PlayerBattingStatsComponent extends BaseballTableComponent<BatterLeaderboardParams, LeaderboardPlayer> implements OnInit {

    @Input({ required: true })
    playerId!: number;
    @Input()
    gamesIdentifier?: string;

    @ViewChild(MatPaginator)
    protected paginator!: MatPaginator;
    @ViewChild(MatSort)
    protected sort!: MatSort;

    dataSource: LeaderboardBattersDataSource;
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
        return Object.keys(this.stats);
    }

    constructor(
        api: BaseballApiService,
        private filterService: BaseballFilterService
    ) {
        super();
        this.dataSource = new LeaderboardBattersDataSource(
            LeaderboardBattersComponent.endpoint,
            ApiMethod.POST,
            api,
            filterService,
            false,
            this.defaultFilters
        );
    }

    public override ngOnInit(): void {
        this.filterService.setFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'playerId', this.playerId);
        this.dataSource.stats$.subscribe(stats => {
            this.stats = stats;
            this.displayedColumns = [
                'year',
                ...this.statNames
            ]
        })
    }

    public setYear(year: number | undefined): void {
        if (this.gamesIdentifier) {
            this.filterService.setFilterValue<PlayerGamesParameters>(this.gamesIdentifier, 'year', year);
        }
    }
}
