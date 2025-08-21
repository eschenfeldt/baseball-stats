import { Component, Input, OnChanges, OnInit, signal, SimpleChanges, ViewChild } from '@angular/core';
import { GamesDataSource, GamesListParams } from './games-datasource';
import { BaseballApiService } from '../baseball-api.service';
import { MatTableModule } from '@angular/material/table';
import { TypeSafeMatCellDef } from '../type-safe-mat-cell-def.directive';
import { TypeSafeMatRowDef } from '../type-safe-mat-row-def.directive';
import { MatPaginatorModule } from '@angular/material/paginator';
import { AsyncPipe, CommonModule } from '@angular/common';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Team } from '../contracts/team';
import { BaseballApiFilter, BaseballFilterService } from '../baseball-filter.service';
import { Utils } from '../utils';
import { SortPipe } from '../sort.pipe';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { GameSummary } from '../contracts/game-summary';
import { Park } from '../contracts/park';
import { InfiniteScrollDirective } from 'ngx-infinite-scroll';
import { BaseballTableComponent } from '../baseball-table-component';
import { ListFiltersComponent } from '../util-components/list-filters/list-filters.component';

@Component({
    selector: 'app-games',
    standalone: true,
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
    templateUrl: './games.component.html',
    styleUrl: './games.component.scss'
})
export class GamesComponent extends BaseballTableComponent<GamesListParams, GameSummary> implements OnInit, OnChanges {

    @Input()
    public team?: Team

    @Input()
    public park?: Park

    protected override paginator = null;
    @ViewChild(MatSort) sort!: MatSort;

    dataSource: GamesDataSource;
    private static readonly allDisplayedColumns: string[] = [
        'date',
        'awayTeam',
        'awayScore',
        'homeTeam',
        'homeScore',
        'location',
    ];
    displayedColumns = GamesComponent.allDisplayedColumns
    protected override get defaultFilters(): BaseballApiFilter {
        return {}
    }

    public condenseInformation: boolean = false

    constructor(
        private api: BaseballApiService,
        private filterService: BaseballFilterService,
        private breakpointObserver: BreakpointObserver,
        private router: Router,
        private route: ActivatedRoute
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
}
