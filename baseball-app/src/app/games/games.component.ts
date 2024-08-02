import { AfterViewInit, Component, Input, OnInit, ViewChild } from '@angular/core';
import { GamesDataSource, GamesListParams, GameSummary } from './games-datasource';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { AsyncPipe, CommonModule } from '@angular/common';
import { BaseballTableComponent } from '../baseball-table-component';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { RouterModule } from '@angular/router';
import { Team } from '../team';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';

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
        CommonModule,
        RouterModule
    ],
    templateUrl: './games.component.html',
    styleUrl: './games.component.scss'
})
export class GamesComponent extends BaseballTableComponent<GamesListParams, GameSummary> implements OnInit, AfterViewInit {

    @Input()
    public team?: Team;

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
    protected override get defaultFilters(): BaseballApiFilter {
        return {
            teamId: this.team?.id
        };
    }

    constructor(
        api: BaseballApiService,
        protected filterService: BaseballFilterService
    ) {
        super('Games');
        this.dataSource = new GamesDataSource("games", ApiMethod.GET, api);
    }

    public override ngOnInit(): void {
        super.ngOnInit();
    }

    public gameTime(game: GameSummary): string {
        if (game.startTime) {
            return this.formatTime(game.startTime);
        } else if (game.scheduledTime) {
            return this.formatTime(game.scheduledTime);
        } else {
            return '';
        }
    }

    public endTime(game: GameSummary): string {
        if (game.endTime) {
            return this.formatTime(game.endTime);
        } else {
            return '';
        }
    }

    private formatTime(datetime: string): string {
        return new Date(datetime).toLocaleTimeString();
    }
}
