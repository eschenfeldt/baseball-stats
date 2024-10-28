import { Component, signal, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatExpansionModule } from '@angular/material/expansion';
import { BatterLeaderboardParams, LeaderboardBattersDataSource } from './leaderboard-batters-datasource';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { BaseballTableComponent } from '../baseball-table-component';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';
import { StatDefCollection } from '../contracts/stat-def';
import { LeaderboardPlayer } from '../contracts/leaderboard-player';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { StatPipe } from '../stat.pipe';

@Component({
    selector: 'app-leaderboard-batters',
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
    templateUrl: './leaderboard-batters.component.html',
    styleUrl: './leaderboard-batters.component.scss'
})
export class LeaderboardBattersComponent extends BaseballTableComponent<BatterLeaderboardParams, LeaderboardPlayer> {

    public static readonly endpoint = 'leaderboard/batting';

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    dataSource: LeaderboardBattersDataSource;
    displayedColumns: string[] = [
        'name'
    ];
    protected override defaultFilters?: BaseballApiFilter = {
        minPlateAppearances: 30
    };
    override defaultPageSize: number = 10;

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
            true,
            this.defaultFilters
        );
        this.dataSource.stats$.subscribe(stats => {
            this.stats = stats;
            this.displayedColumns = [
                'name',
                ...this.statNames
            ]
        })
    }

    readonly filterOpenState = signal(false);

    public get filterSummary(): string {
        if (this.filterOpenState()) {
            return '';
        } else {
            return `Min PA: ${this.minPA}`;
        }
    }

    public get minPA(): number {
        return this.filterService.getFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'minPlateAppearances');
    }
    public set minPA(value: number) {
        this.filterService.setFilterValue<BatterLeaderboardParams>(this.uniqueIdentifier, 'minPlateAppearances', value || 0);
    }
}
