import { Component, ViewChild } from '@angular/core';
import { BaseballTableComponent } from '../baseball-table-component';
import { PagedApiParameters } from '../paged-api-parameters';
import { Team } from '../team';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BaseballDataSource } from '../baseball-data-source';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { TeamsDataSource, TeamSummary } from './teams-datasource';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { AsyncPipe, CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BaseballFilterService, BaseballApiFilter } from '../baseball-filter.service';

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
        RouterModule
    ],
    templateUrl: './teams.component.html',
    styleUrl: './teams.component.scss'
})
export class TeamsComponent extends BaseballTableComponent<PagedApiParameters, TeamSummary> {

    override componentName: string = 'Teams';

    @ViewChild(MatPaginator)
    protected paginator!: MatPaginator;
    @ViewChild(MatSort)
    protected sort!: MatSort;

    dataSource: TeamsDataSource;
    displayedColumns = [
        'team',
        'games',
        'wins',
        'losses'
    ]
    protected override defaultFilters?: BaseballApiFilter = {};

    constructor(
        api: BaseballApiService,
        protected filterService: BaseballFilterService
    ) {
        super('Teams');
        this.dataSource = new TeamsDataSource('teams/summaries', ApiMethod.GET, api);
        this.filterService = filterService
    }
}
