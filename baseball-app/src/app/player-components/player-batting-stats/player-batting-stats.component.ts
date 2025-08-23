import { Component, Input, OnChanges, OnInit, ViewChild } from '@angular/core';
import { BaseballTableComponent } from '../../baseball-table-component';
import { BatterLeaderboardParams, LeaderboardBattersDataSource } from '../../leaderboard-batters/leaderboard-batters-datasource';
import { LeaderboardPlayer } from '../../contracts/leaderboard-player';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballApiFilter, BaseballFilterService } from '../../baseball-filter.service';
import { ApiMethod, BaseballApiService } from '../../baseball-api.service';
import { LeaderboardBattersComponent } from '../../leaderboard-batters/leaderboard-batters.component';
import { StatDefCollection } from '../../contracts/stat-def';
import { AsyncPipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../../type-safe-mat-row-def.directive';
import { StatPipe } from '../../stat.pipe';
import { ActivatedRoute, Router } from '@angular/router';

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
export class PlayerBattingStatsComponent extends BaseballTableComponent<BatterLeaderboardParams, LeaderboardPlayer> implements OnInit, OnChanges {

    @Input({ required: true })
    playerId!: number;

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

    private get teamId(): number | undefined {
        return this.filterService.getFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'teamId')
    }
    private set teamId(value: number | undefined) {
        this.filterService.setFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'teamId', value)
    }

    private get parkId(): number | undefined {
        return this.filterService.getFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'parkId')
    }
    private set parkId(value: number | undefined) {
        this.filterService.setFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'parkId', value)
    }

    private get year(): number | undefined {
        return this.filterService.getFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'year')
    }
    private set year(value: number | undefined) {
        this.filterService.setFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'year', value)
    }

    constructor(
        api: BaseballApiService,
        private filterService: BaseballFilterService,
        private router: Router,
        private route: ActivatedRoute
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
        this.route.queryParams.subscribe((params: BatterLeaderboardParams) => {
            if (params.teamId && +params.teamId !== this.teamId) {
                this.teamId = +params.teamId
            } else if (params.teamId == null && this.teamId) {
                this.teamId = undefined
            }
            if (params.parkId && +params.parkId !== this.parkId) {
                this.parkId = +params.parkId
            } else if (params.parkId == null && this.parkId) {
                this.parkId = undefined
            }
            if (params.year && +params.year !== this.year) {
                this.year = +params.year
            } else if (params.year == null && this.year) {
                this.year = undefined
            }
        })
    }

    public ngOnChanges(): void {
        if (this.playerId) {
            this.initialize();
        }
    }

    private initialize(): void {
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
        this.router.navigate([], { queryParams: { year: year }, queryParamsHandling: 'merge' })
    }
}
