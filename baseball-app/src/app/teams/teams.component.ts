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
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BaseballFilterService, BaseballApiFilter } from '../baseball-filter.service';
import { Team } from '../contracts/team';
import { Utils } from '../utils';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';

@Component({
    selector: 'app-teams',
    standalone: true,
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatPaginatorModule,
        MatSortModule,
        AsyncPipe,
        CommonModule,
        RouterModule,
        InfiniteScrollDirective
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
    displayedColumns = [
        'team',
        'lastGame',
        'games',
        'wins',
        'losses',
        'parks'
    ]
    protected override defaultFilters?: BaseballApiFilter = {};

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
    }

    lastGameDate(team: TeamSummary): string {
        return Utils.formatDate(team.lastGameDate);
    }
}
