import { Component, ViewChild } from '@angular/core';
import { BaseballTableComponent } from '../baseball-table-component';
import { PagedApiParameters } from '../paged-api-parameters';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { TeamsDataSource, TeamSummary } from './teams-datasource';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BaseballFilterService, BaseballApiFilter } from '../baseball-filter.service';
import { Team } from '../contracts/team';
import { Utils } from '../utils';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { FilterOption, ListFilterParams, ListFiltersComponent } from '../util-components/list-filters/list-filters.component';

@Component({
    selector: 'app-teams',
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatPaginatorModule,
        MatSortModule,
        CommonModule,
        RouterModule,
        InfiniteScrollDirective,
        ListFiltersComponent
    ],
    templateUrl: './teams.component.html',
    styleUrl: './teams.component.scss'
})
export class TeamsComponent extends BaseballTableComponent<PagedApiParameters, TeamSummary> {

    @ViewChild(MatPaginator)
    protected paginator!: MatPaginator
    @ViewChild(MatSort)
    protected sort!: MatSort;

    dataSource: TeamsDataSource;
    private static readonly allColumns = [
        'team',
        'lastGame',
        'games',
        'wins',
        'losses',
        'parks'
    ]
    displayedColumns = TeamsComponent.allColumns
    protected override defaultFilters?: BaseballApiFilter = {};
    readonly hideTeamFilter = FilterOption.hide

    backgroundColor(team: Team): string {
        return Utils.transparentTeamColor(team, 20);
    }

    constructor(
        api: BaseballApiService,
        protected filterService: BaseballFilterService
    ) {
        super();
        this.dataSource = new TeamsDataSource(
            'teams/summaries',
            ApiMethod.GET,
            api,
            filterService,
            true,
            this.defaultFilters
        );
        this.filterService.filtersChanged$(this.uniqueIdentifier).subscribe(() => {
            if (this.filterService.getFilterValue<ListFilterParams>(this.uniqueIdentifier, 'parkId')) {
                // hide this column that would just always show 1 with a misleading link
                this.displayedColumns = TeamsComponent.allColumns.filter(c => c !== 'parks')
            } else {
                this.displayedColumns = TeamsComponent.allColumns
            }
        })
    }

    lastGameDate(team: TeamSummary): string {
        return Utils.formatDate(team.lastGameDate);
    }
}
