import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { LeaderboardBattersComponent } from '../leaderboard-batters/leaderboard-batters.component';
import { LeaderboardPitchersComponent } from '../leaderboard-pitchers/leaderboard-pitchers.component';
import { BaseballFilterService } from '../baseball-filter.service';
import { PitcherLeaderboardParams } from '../leaderboard-pitchers/leaderboard-pitchers-datasource';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { BatterLeaderboardParams } from '../leaderboard-batters/leaderboard-batters-datasource';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { zip } from 'rxjs';
import { MatSelectModule } from '@angular/material/select';
import { AsyncPipe } from '@angular/common';
import { SortPipe } from '../sort.pipe';
import { MatExpansionModule } from '@angular/material/expansion';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { ListFiltersComponent } from '../util-components/list-filters/list-filters.component';

@Component({
    selector: 'app-leaders',
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
        SortPipe,
        ListFiltersComponent
    ],
    templateUrl: './leaders.component.html',
    styleUrl: './leaders.component.scss'
})
export class LeadersComponent implements AfterViewInit {

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

    public condenseInformation: boolean = false;
    public get batterLabel(): string {
        return this.condenseInformation ? "B" : "Batters";
    }
    public get pitcherLabel(): string {
        return this.condenseInformation ? "P" : "Pitchers";
    }

    public get batterIdentifier(): string {
        return LeaderboardBattersComponent.endpoint;
    }
    public get pitcherIdentifier(): string {
        return LeaderboardPitchersComponent.endpoint;
    }

    public constructor(
        private filterService: BaseballFilterService,
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

    public ngAfterViewInit(): void {
        const allCounts = zip(this.batters.dataSource.totalCount$, this.pitchers.dataSource.totalCount$);
        allCounts.subscribe(counts => {
            if (counts.every(c => c < 5 && c > 0)) {
                this.showTables = [Table.batters, Table.pitchers];
            } else if (counts[0] === 0 && counts[1] > 0) {
                this.showTables = [Table.pitchers];
            } else if (counts[0] > 0 && counts[1] === 0) {
                this.showTables = [Table.batters];
            } else if (counts.every(c => c === 0)) {
                // expand playing time filters when there are no results
                this.filterService.setFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'minInningsPitched', 0);
                this.filterService.setFilterValue<BatterLeaderboardParams>(LeaderboardBattersComponent.endpoint, 'minPlateAppearances', 0);
            }
        });
    }
}

enum Table {
    batters = 'batters',
    pitchers = 'pitchers'
}
