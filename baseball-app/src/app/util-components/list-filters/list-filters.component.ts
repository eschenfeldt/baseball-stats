import { Component, Input, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Park } from '../../contracts/park';
import { Team } from '../../contracts/team';
import { BaseballFilterService } from '../../baseball-filter.service';
import { ActivatedRoute, Router } from '@angular/router';
import { mergeMap, Observable } from 'rxjs';
import { PagedApiParameters } from '../../paged-api-parameters';
import { BaseballApiService } from '../../baseball-api.service';
import { AsyncPipe } from '@angular/common';
import { SortPipe } from '../../sort.pipe';

export interface ListFilterParams extends PagedApiParameters {
    teamId?: number;
    parkId?: number;
    year?: number;
    playerId?: number;
    playerSearch?: string;
}

export enum FilterOption {
    hide
}

@Component({
    selector: 'app-list-filters',
    imports: [
        FormsModule,
        MatInputModule,
        MatFormFieldModule,
        MatSelectModule,
        MatExpansionModule,
        AsyncPipe,
        SortPipe
    ],
    templateUrl: './list-filters.component.html',
    styleUrl: './list-filters.component.scss'
})
export class ListFiltersComponent implements OnInit {

    @Input({ required: true })
    public uniqueIdentifier!: string

    @Input()
    public secondaryUniqueIdentifiers?: string[]

    @Input()
    public team?: Team | FilterOption

    @Input()
    public park?: Park | FilterOption

    @Input()
    public playerId?: number

    @Input()
    public includePlayerSearch: boolean = false

    public yearOptions$?: Observable<number[]>
    public teamOptions$?: Observable<Team[]>
    public parkOptions$?: Observable<Park[]>

    private teamCache: { [id: number]: Team } = {}
    private parkCache: { [id: number]: Park } = []

    public get selectedYear(): number | undefined {
        return +this.filterService.getFilterValue<ListFilterParams>(this.uniqueIdentifier, 'year');
    }
    public set selectedYear(value: number | undefined) {
        this.filterService.setFilterValue<ListFilterParams>(this.uniqueIdentifier, 'year', value);
        if (this.secondaryUniqueIdentifiers) {
            this.secondaryUniqueIdentifiers.forEach(ui => {
                this.filterService.setFilterValue<ListFilterParams>(ui, 'year', value)
            })
        }
        this.router.navigate([], { queryParams: { year: value }, queryParamsHandling: 'merge' })
    }

    public get selectedTeamId(): number | undefined {
        return +this.filterService.getFilterValue<ListFilterParams>(this.uniqueIdentifier, 'teamId')
    }
    public set selectedTeamId(value: number | undefined) {
        this.filterService.setFilterValue<ListFilterParams>(this.uniqueIdentifier, 'teamId', value)
        if (this.secondaryUniqueIdentifiers) {
            this.secondaryUniqueIdentifiers.forEach(ui => {
                this.filterService.setFilterValue<ListFilterParams>(ui, 'teamId', value)
            })
        }
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
        return +this.filterService.getFilterValue<ListFilterParams>(this.uniqueIdentifier, 'parkId')
    }
    public set selectedParkId(value: number | undefined) {
        this.filterService.setFilterValue<ListFilterParams>(this.uniqueIdentifier, 'parkId', value)
        if (this.secondaryUniqueIdentifiers) {
            this.secondaryUniqueIdentifiers.forEach(ui => {
                this.filterService.setFilterValue<ListFilterParams>(ui, 'parkId', value)
            })
        }
        this.router.navigate([], { queryParams: { parkId: value }, queryParamsHandling: 'merge' })
    }
    private get selectedPark(): Park | undefined {
        if (this.selectedParkId) {
            return this.parkCache[this.selectedParkId]
        } else {
            return undefined
        }
    }

    public get search(): string {
        return this.filterService.getFilterValue<ListFilterParams>(this.uniqueIdentifier, 'playerSearch');
    }
    public set search(val: string) {
        this.filterService.setFilterValue<ListFilterParams>(this.uniqueIdentifier, 'playerSearch', val);
        if (this.secondaryUniqueIdentifiers) {
            this.secondaryUniqueIdentifiers.forEach(ui => {
                this.filterService.setFilterValue<ListFilterParams>(ui, 'playerSearch', val)
            })
        }
    }

    public constructor(
        private api: BaseballApiService,
        private filterService: BaseballFilterService,
        private router: Router,
        private route: ActivatedRoute
    ) { }

    public ngOnInit(): void {
        this.route.queryParams.subscribe((params: ListFilterParams) => {
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
            if (params.year && +params.year !== this.selectedYear) {
                this.selectedYear = +params.year
            } else if (params.year == null && this.selectedYear) {
                this.selectedYear = undefined
            }
        })
        const updateTriggers$ = this.filterService.filtersChanged$(this.uniqueIdentifier);
        this.yearOptions$ = updateTriggers$.pipe(mergeMap(() => {
            const teamId = this.team === FilterOption.hide ? undefined : this.team?.id;
            const parkId = this.park === FilterOption.hide ? undefined : this.park?.id;
            const yearParams: ListFilterParams = { teamId: teamId, parkId: parkId, playerId: this.playerId }
            this.filterService.updateParamsFromFilters(this.uniqueIdentifier, yearParams)
            return this.api.makeApiGet<number[]>('games/years', yearParams)
        }))
        this.teamOptions$ = updateTriggers$.pipe(mergeMap(() => {
            const parkId = this.park === FilterOption.hide ? undefined : this.park?.id;
            const teamParams: ListFilterParams = { parkId: parkId, playerId: this.playerId }
            this.filterService.updateParamsFromFilters(this.uniqueIdentifier, teamParams)
            return this.api.makeApiGet<Team[]>('teams', teamParams)
        }))
        this.parkOptions$ = updateTriggers$.pipe(mergeMap(() => {
            const teamId = this.team === FilterOption.hide ? undefined : this.team?.id;
            const parkParams: ListFilterParams = { teamId: teamId, playerId: this.playerId }
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
