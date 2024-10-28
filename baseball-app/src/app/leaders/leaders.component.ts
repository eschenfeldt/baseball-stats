import { AfterViewInit, Component, OnInit, signal, ViewChild } from '@angular/core';
import { LeaderboardBattersComponent } from '../leaderboard-batters/leaderboard-batters.component';
import { LeaderboardPitchersComponent } from '../leaderboard-pitchers/leaderboard-pitchers.component';
import { BaseballFilterService } from '../baseball-filter.service';
import { PitcherLeaderboardParams } from '../leaderboard-pitchers/leaderboard-pitchers-datasource';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { BatterLeaderboardParams } from '../leaderboard-batters/leaderboard-batters-datasource';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { Observable, zip } from 'rxjs';
import { MatSelectModule } from '@angular/material/select';
import { AsyncPipe } from '@angular/common';
import { BaseballApiService } from '../baseball-api.service';
import { SortPipe } from '../sort.pipe';
import { MatExpansionModule } from '@angular/material/expansion';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';

@Component({
    selector: 'app-leaders',
    standalone: true,
    imports: [
        LeaderboardBattersComponent,
        LeaderboardPitchersComponent,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonToggleModule,
        MatSelectModule,
        MatExpansionModule,
        AsyncPipe,
        SortPipe
    ],
    templateUrl: './leaders.component.html',
    styleUrl: './leaders.component.scss'
})
export class LeadersComponent implements OnInit, AfterViewInit {

    @ViewChild(LeaderboardBattersComponent)
    batters!: LeaderboardBattersComponent;

    @ViewChild(LeaderboardPitchersComponent)
    pitchers!: LeaderboardPitchersComponent;

    showTables: Table[] = [Table.batters]
    public get showBatters(): boolean {
        return this.showTables.some(t => t === Table.batters);
    }
    public get showPitchers(): boolean {
        return this.showTables.some(t => t === Table.pitchers);
    }

    public get search(): string {
        return this.filterService.getFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'playerSearch');
    }
    public set search(val: string) {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'playerSearch', val);
        this.filterService.setFilterValue<BatterLeaderboardParams>(LeaderboardBattersComponent.endpoint, 'playerSearch', val);
    }

    public yearOptions$?: Observable<number[]>;

    public get selectedYear(): number | undefined {
        return this.filterService.getFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'year');
    }
    public set selectedYear(value: number) {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'year', value);
        this.filterService.setFilterValue<BatterLeaderboardParams>(LeaderboardBattersComponent.endpoint, 'year', value);
    }

    public condenseInformation: boolean = false;
    public get batterLabel(): string {
        return this.condenseInformation ? "B" : "Batters";
    }
    public get pitcherLabel(): string {
        return this.condenseInformation ? "P" : "Pitchers";
    }

    public constructor(
        private filterService: BaseballFilterService,
        private api: BaseballApiService,
        private breakpointObserver: BreakpointObserver
    ) {
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

    public ngOnInit(): void {
        this.yearOptions$ = this.api.makeApiGet<number[]>('games/years', {});
    }

    public ngAfterViewInit(): void {
        const allCounts = zip(this.batters.dataSource.totalCount$, this.pitchers.dataSource.totalCount$);
        allCounts.subscribe(counts => {
            if (counts.every(c => c < 5 && c > 0)) {
                this.showTables = [Table.batters, Table.pitchers];
            } else if (counts[0] === 0 && counts[1] > 0) {
                this.showTables = [Table.pitchers];
            } else if (counts[0] > 0 && counts[1] === 0) {
                this.showTables = [Table.batters];
            } else if (counts.every(c => c === 0) && this.search) {
                // expand filters to try to identify a search result
                this.filterService.setFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'minInningsPitched', 0);
                this.filterService.setFilterValue<BatterLeaderboardParams>(LeaderboardBattersComponent.endpoint, 'minPlateAppearances', 0);
            }
        });
    }

    readonly filterOpenState = signal(false);

    public get filterSummary(): string {
        if (this.filterOpenState()) {
            return '';
        } else if (this.search && this.selectedYear) {
            return `${this.search} ${this.selectedYear}`;
        } else if (this.search) {
            return `Search: ${this.search}`;
        } else if (this.selectedYear) {
            return `Year: ${this.selectedYear}`;
        } else {
            return '';
        }
    }
}

enum Table {
    batters = 'batters',
    pitchers = 'pitchers'
}
