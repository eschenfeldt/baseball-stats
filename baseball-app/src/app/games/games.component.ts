import { Component, Input, OnChanges, OnInit, signal, SimpleChanges, ViewChild } from '@angular/core';
import { GamesDataSource, GamesListParams } from './games-datasource';
import { BaseballApiService } from '../baseball-api.service';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { AsyncPipe, CommonModule } from '@angular/common';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { RouterModule } from '@angular/router';
import { Team } from '../contracts/team';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';
import { Observable } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { Utils } from '../utils';
import { SortPipe } from '../sort.pipe';
import { MatExpansionModule } from '@angular/material/expansion';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { GameSummary } from '../contracts/game-summary';
import { Park } from '../contracts/park';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { BaseballScrollTableComponent } from '../baseball-scroll-table-component';

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
        SortPipe,
        CommonModule,
        RouterModule,
        FormsModule,
        MatInputModule,
        MatFormFieldModule,
        MatSelectModule,
        MatExpansionModule,
        InfiniteScrollDirective
    ],
    templateUrl: './games.component.html',
    styleUrl: './games.component.scss'
})
export class GamesComponent extends BaseballScrollTableComponent<GamesListParams, GameSummary> implements OnInit, OnChanges {

    @Input()
    public team?: Team

    @Input()
    public park?: Park

    @ViewChild(MatSort) sort!: MatSort;

    dataSource: GamesDataSource;
    private static readonly allDisplayedColumns: string[] = [
        'date',
        'awayTeam',
        'homeTeam',
        'location',
    ];
    displayedColumns = GamesComponent.allDisplayedColumns;
    protected override get defaultFilters(): BaseballApiFilter {
        return {};
    }

    public condenseInformation: boolean = false;

    public yearOptions$?: Observable<number[]>;

    public get selectedYear(): number | undefined {
        return this.filterService.getFilterValue<GamesListParams>(this.uniqueIdentifier, 'year');
    }
    public set selectedYear(value: number) {
        this.filterService.setFilterValue<GamesListParams>(this.uniqueIdentifier, 'year', value);
    }

    constructor(
        private api: BaseballApiService,
        private filterService: BaseballFilterService,
        private breakpointObserver: BreakpointObserver
    ) {
        super();
        this.dataSource = new GamesDataSource(api, filterService);

        this.breakpointObserver.observe([
            Breakpoints.HandsetPortrait
        ]).subscribe(result => {
            if (result.matches) {
                this.condenseInformation = true;
            } else {
                this.condenseInformation = false;
            }
        });
    }

    public override ngOnInit(): void {
        super.ngOnInit();
        this.yearOptions$ = this.api.makeApiGet<number[]>('games/years', { teamId: this.team?.id, parkId: this.park?.id });
    }

    ngOnChanges(_changes: SimpleChanges): void {
        if (this.team) {
            this.filterService.setFilterValue<GamesListParams>(this.uniqueIdentifier, 'teamId', this.team.id);
        } else {
            this.filterService.unsetFilterValue<GamesListParams>(this.uniqueIdentifier, 'teamId');
        }
        if (this.park) {
            this.filterService.setFilterValue<GamesListParams>(this.uniqueIdentifier, 'parkId', this.park.id);
            this.displayedColumns = GamesComponent.allDisplayedColumns.filter(col => col !== 'location');
        } else {
            this.filterService.unsetFilterValue<GamesListParams>(this.uniqueIdentifier, 'parkId');
            this.displayedColumns = GamesComponent.allDisplayedColumns;
        }
    }

    public gameDate(game: GameSummary): string {
        return Utils.formatDate(game.date);
    }

    public gameTime(game: GameSummary): string {
        if (game.startTime) {
            return Utils.formatTime(game.startTime);
        } else if (game.scheduledTime) {
            return Utils.formatTime(game.scheduledTime);
        } else {
            return '';
        }
    }

    public endTime(game: GameSummary): string {
        if (game.endTime) {
            return Utils.formatTime(game.endTime);
        } else {
            return '';
        }
    }

    readonly filterOpenState = signal(false);

    public get filterSummary(): string {
        if (this.filterOpenState()) {
            return '';
        } else if (this.selectedYear) {
            return `Year: ${this.selectedYear}`;
        } else {
            return '';
        }
    }
}
