import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { GamesDataSource, GamesListParams, GameSummary } from './games-datasource';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { AsyncPipe, CommonModule } from '@angular/common';
import { BaseballTableComponent } from '../baseball-table-component';
import { MatSort, MatSortModule } from '@angular/material/sort';

@Component({
    selector: 'app-games',
    standalone: true,
    imports: [
        MatTableModule,
        TypeSafeMatCellDef,
        TypeSafeMatRowDef,
        MatPaginatorModule,
        MatSortModule,
        AsyncPipe,
        CommonModule
    ],
    templateUrl: './games.component.html',
    styleUrl: './games.component.scss'
})
export class GamesComponent extends BaseballTableComponent<GamesListParams, GameSummary> implements OnInit, AfterViewInit {

    @ViewChild(MatPaginator) paginator!: MatPaginator;
    @ViewChild(MatSort) sort!: MatSort;

    dataSource: GamesDataSource;
    displayedColumns: string[] = [
        'date',
        'awayTeam',
        'awayScore',
        'homeTeam',
        'homeScore'
    ];

    constructor(
        private api: BaseballApiService
    ) {
        super();
        this.dataSource = new GamesDataSource("games", ApiMethod.GET, api);
    }

}
