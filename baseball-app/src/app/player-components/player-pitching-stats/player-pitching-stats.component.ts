import { Component, EventEmitter, Input, OnChanges, OnInit, Output, ViewChild } from '@angular/core';
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
import { ActivatedRoute } from '@angular/router';

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

    private get teamId(): number | undefined {
        return this.filterService.getFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'teamId')
    }
    private set teamId(value: number | undefined) {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'teamId', value)
    }

    private get parkId(): number | undefined {
        return this.filterService.getFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'parkId')
    }
    private set parkId(value: number | undefined) {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'parkId', value)
    }

    private get year(): number | undefined {
        return this.filterService.getFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'year')
    }
    private set year(value: number | undefined) {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'year', value)
    }

    constructor(
        api: BaseballApiService,
        private filterService: BaseballFilterService,
        private route: ActivatedRoute
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
        this.route.queryParams.subscribe((params: PitcherLeaderboardParams) => {
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
