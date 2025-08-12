import { Component, ViewChild } from '@angular/core';
import { ParksDataSource, ParkSummary, ParkSummaryParameters } from './parks-datasource';
import { BaseballTableComponent } from '../baseball-table-component';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { Utils } from '../utils';
import { MatTableModule } from '@angular/material/table';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';

@Component({
    selector: 'app-parks',
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
    templateUrl: './parks.component.html',
    styleUrl: './parks.component.scss'
})
export class ParksComponent extends BaseballTableComponent<ParkSummaryParameters, ParkSummary> {

    @ViewChild(MatPaginator)
    protected paginator!: MatPaginator
    @ViewChild(MatSort)
    protected sort!: MatSort;

    dataSource: ParksDataSource;
    displayedColumns = [
        'park',
        'games',
        'firstGame',
        'lastGame',
        'wins',
        'losses',
        'teams'
    ]
    protected override defaultFilters?: BaseballApiFilter = {};


    constructor(
        api: BaseballApiService,
        protected filterService: BaseballFilterService
    ) {
        super();
        this.dataSource = new ParksDataSource(
            'park/summaries',
            ApiMethod.GET,
            api,
            filterService,
            true,
            this.defaultFilters
        );
    }

    firstGameDate(park: ParkSummary): string {
        return Utils.formatDate(park.firstGameDate);
    }

    lastGameDate(park: ParkSummary): string {
        return Utils.formatDate(park.lastGameDate);
    }
}
