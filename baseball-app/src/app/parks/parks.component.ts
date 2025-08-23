import { Component, ViewChild } from '@angular/core';
import { ParksDataSource, ParkSummary, ParkSummaryParameters } from './parks-datasource';
import { BaseballTableComponent } from '../baseball-table-component';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { Utils } from '../utils';
import { MatTableModule } from '@angular/material/table';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { FilterOption, ListFiltersComponent } from '../util-components/list-filters/list-filters.component';

@Component({
    selector: 'app-parks',
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatSortModule,
        AsyncPipe,
        CommonModule,
        RouterModule,
        InfiniteScrollDirective,
        ListFiltersComponent
    ],
    templateUrl: './parks.component.html',
    styleUrl: './parks.component.scss'
})
export class ParksComponent extends BaseballTableComponent<ParkSummaryParameters, ParkSummary> {

    protected override paginator = null;
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
    public readonly hideParkFilter: FilterOption = FilterOption.hide;

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
