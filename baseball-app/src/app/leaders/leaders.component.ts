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

@Component({
    selector: 'app-leaders',
    standalone: true,
    imports: [
        LeaderboardBattersComponent,
        LeaderboardPitchersComponent,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonToggleModule
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

    public get search(): string {
        return this.filterService.getFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'playerSearch');
    }
    public set search(val: string) {
        this.filterService.setFilterValue<PitcherLeaderboardParams>(LeaderboardPitchersComponent.endpoint, 'playerSearch', val);
        this.filterService.setFilterValue<BatterLeaderboardParams>(LeaderboardBattersComponent.endpoint, 'playerSearch', val);
    }

    public constructor(
        private filterService: BaseballFilterService
    ) { }

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

}

enum Table {
    batters = 'batters',
    pitchers = 'pitchers'
}
