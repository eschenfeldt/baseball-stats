import { Component, OnInit } from '@angular/core';
import { param } from '../param.decorator';
import { BASEBALL_ROUTES } from '../app.routes';
import { Observable, switchMap } from 'rxjs';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { Team } from '../contracts/team';
import { AsyncPipe } from '@angular/common';
import { GamesComponent } from '../games/games.component';
import { SummaryStatsComponent } from '../util-components/summary-stats/summary-stats.component';
import { SummaryStat } from '../contracts/summary-stat';
import { StatCategory } from '../contracts/stat-category';

@Component({
    selector: 'app-team',
    standalone: true,
    imports: [
        AsyncPipe,
        GamesComponent,
        SummaryStatsComponent
    ],
    templateUrl: './team.component.html',
    styleUrl: './team.component.scss'
})
export class TeamComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.TEAM>("teamId")
    public teamId$!: Observable<number>
    team$?: Observable<Team>
    summaryStats$?: Observable<SummaryStat[]>
    get generalStatCategory(): StatCategory {
        return StatCategory.general;
    }

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.team$ = this.teamId$.pipe(switchMap(teamId => {
            return this.api.makeApiGet<Team>(`teams/${teamId}`, true, false);
        }));
        this.summaryStats$ = this.teamId$.pipe(switchMap(teamId => {
            return this.api.makeApiGet<SummaryStat[]>('games/summary-stats', { teamId: teamId });
        }))
    }
}
