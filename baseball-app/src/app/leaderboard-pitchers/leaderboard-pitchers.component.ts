import { Component, signal, ViewChild } from '@angular/core';
import { BaseballTableComponent } from '../baseball-table-component';
import { LeaderboardPitchersDataSource, PitcherLeaderboardParams } from './leaderboard-pitchers-datasource';
import { LeaderboardPlayer } from '../contracts/leaderboard-player';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';
import { StatDefCollection } from '../contracts/stat-def';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { AsyncPipe, CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { FormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Utils } from '../utils';
import { StatPipe } from '../stat.pipe';

@Component({
    selector: 'app-leaderboard-pitchers',
    standalone: true,
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatPaginatorModule,
        MatSortModule,
        MatExpansionModule,
        AsyncPipe,
        StatPipe,
        CommonModule,
        RouterModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule
    ],
    templateUrl: './leaderboard-pitchers.component.html',
    styleUrl: './leaderboard-pitchers.component.scss'
})
export class LeaderboardPitchersComponent extends BaseballTableComponent<PitcherLeaderboardParams, LeaderboardPlayer> {

    public static readonly endpoint = 'leaderboard/pitching';

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    dataSource: LeaderboardPitchersDataSource;
    displayedColumns: string[] = [
        'name',
        'inningsPitched'
    ];
    protected override defaultFilters?: BaseballApiFilter = {
        minInningsPitched: 10
    };
    override defaultPageSize: number = 10;

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
            true,
            this.defaultFilters
        );
        this.dataSource.stats$.subscribe(stats => {
            this.stats = stats;
            this.displayedColumns = [
                'name',
                'inningsPitched',
                ...this.statNames
            ]
        })
    }


    fullInningsPitched(pitcher: LeaderboardPlayer): string {
        return Utils.fullInningsPitched(pitcher.stats);
    }
    partialInningsPitched(pitcher: LeaderboardPlayer): string {
        return Utils.partialInningsPitched(pitcher.stats);
    }

    readonly filterOpenState = signal(false);

    public get filterSummary(): string {
        if (this.filterOpenState()) {
            return '';
        } else {
            return `Min IP: ${this.minIP}`;
        }
    }

    public get minIP(): number {
        return this.filterService.getFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'minInningsPitched');
    }
    public set minIP(value: number) {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(this.uniqueIdentifier, 'minInningsPitched', value || 0);
    }
}
