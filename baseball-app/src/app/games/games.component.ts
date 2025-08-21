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
import { BaseballTableComponent } from '../baseball-table-component';
import { mergeMap } from 'rxjs/operators';

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

    public yearOptions$?: Observable<number[]>
    public teamOptions$?: Observable<Team[]>
    public parkOptions$?: Observable<Park[]>

    private teamCache: { [id: number]: Team } = {}
    private parkCache: { [id: number]: Park } = []

    public get selectedYear(): number | undefined {
        return +this.filterService.getFilterValue<GamesListParams>(this.uniqueIdentifier, 'year');
    }
    public set selectedYear(value: number) {
        this.filterService.setFilterValue<GamesListParams>(this.uniqueIdentifier, 'year', value);
    }

    public get selectedTeamId(): number | undefined {
        return +this.filterService.getFilterValue<GamesListParams>(this.uniqueIdentifier, 'teamId')
    }
    public set selectedTeamId(value: number | undefined) {
        this.filterService.setFilterValue<GamesListParams>(this.uniqueIdentifier, 'teamId', value)
        this.router.navigate([], { queryParams: { teamId: value }, queryParamsHandling: 'merge' })
    }
    private get selectedTeam(): Team | undefined {
        if (this.selectedTeamId) {
            return this.teamCache[this.selectedTeamId]
        } else {
            return undefined
        }
    }

    public get selectedParkId(): number | undefined {
        return +this.filterService.getFilterValue<GamesListParams>(this.uniqueIdentifier, 'parkId')
    }
    public set selectedParkId(value: number | undefined) {
        this.filterService.setFilterValue<GamesListParams>(this.uniqueIdentifier, 'parkId', value)
        this.router.navigate([], { queryParams: { parkId: value }, queryParamsHandling: 'merge' })
    }
    private get selectedPark(): Park | undefined {
        if (this.selectedParkId) {
            return this.parkCache[this.selectedParkId]
        } else {
            return undefined
        }
    }

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
        this.route.queryParams.subscribe((params: GamesListParams) => {
            if (this.team == null && params.teamId && +params.teamId !== this.selectedTeamId) {
                this.selectedTeamId = +params.teamId
            } else if (this.team == null && params.teamId == null && this.selectedTeamId) {
                this.selectedTeamId = undefined
            }
            if (params.parkId && this.park == null && +params.parkId !== this.selectedParkId) {
                this.selectedParkId = +params.parkId
            } else if (this.park == null && params.parkId == null && this.selectedParkId) {
                this.selectedParkId = undefined
            }
        })
        const updateTriggers$ = this.filterService.filtersChanged$(this.uniqueIdentifier);
        this.yearOptions$ = updateTriggers$.pipe(mergeMap(() => {
            const yearParams: GamesListParams = { teamId: this.team?.id, parkId: this.park?.id }
            this.filterService.updateParamsFromFilters(this.uniqueIdentifier, yearParams)
            return this.api.makeApiGet<number[]>('games/years', yearParams)
        }))
        this.teamOptions$ = updateTriggers$.pipe(mergeMap(() => {
            const teamParams: GamesListParams = { parkId: this.park?.id }
            this.filterService.updateParamsFromFilters(this.uniqueIdentifier, teamParams)
            return this.api.makeApiGet<Team[]>('teams', teamParams)
        }))
        this.parkOptions$ = updateTriggers$.pipe(mergeMap(() => {
            const parkParams: GamesListParams = { teamId: this.team?.id }
            this.filterService.updateParamsFromFilters(this.uniqueIdentifier, parkParams)
            return this.api.makeApiGet<Park[]>('park', parkParams)
        }))

        this.teamOptions$.subscribe(teams => {
            teams.forEach(t => {
                this.teamCache[t.id] = t
            })
        })
        this.parkOptions$.subscribe(parks => {
            parks.forEach(p => {
                this.parkCache[p.id] = p
            })
        })
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
        let summary = '';
        if (this.filterOpenState()) {
            return summary;
        }
        if (this.selectedYear) {
            summary += `Year: ${this.selectedYear}`;
        }
        if (this.team == null && this.selectedTeam) {
            if (summary) {
                summary += ', '
            }
            summary += `Team: ${this.selectedTeam.abbreviation}`
        }
        if (this.park == null && this.selectedPark) {
            if (summary) {
                summary += ', '
            }
            summary += `Park: ${this.selectedPark.name}`
        }

        return summary;
    }
}
