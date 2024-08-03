import { Component, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BatterLeaderboardParams, LeaderboardBatter, LeaderboardBattersDataSource } from './leaderboard-batters-datasource';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { BaseballTableComponent } from '../baseball-table-component';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';

@Component({
    selector: 'app-leaderboard-batters',
    standalone: true,
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatPaginatorModule,
        MatSortModule,
        AsyncPipe,
        CommonModule,
        RouterModule
    ],
    templateUrl: './leaderboard-batters.component.html',
    styleUrl: './leaderboard-batters.component.scss'
})
export class LeaderboardBattersComponent extends BaseballTableComponent<BatterLeaderboardParams, LeaderboardBatter> {

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    dataSource: LeaderboardBattersDataSource;
    displayedColumns: string[] = [
        'name',
        'games',
        'atBats',
        'hits',
        'battingAverage'
    ];
    protected override defaultFilters?: BaseballApiFilter = {};

    constructor(
        api: BaseballApiService,
        filterService: BaseballFilterService
    ) {
        super();
        this.dataSource = new LeaderboardBattersDataSource(
            'leaderboard/batting',
            ApiMethod.POST,
            api,
            filterService,
            true,
            this.defaultFilters
        );
    }
}
