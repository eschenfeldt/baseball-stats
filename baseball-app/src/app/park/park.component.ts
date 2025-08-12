import { Component, OnInit } from '@angular/core';
import { param } from '../param.decorator';
import { Observable, switchMap } from 'rxjs';
import { BASEBALL_ROUTES } from '../app.routes';
import { BaseballApiService } from '../baseball-api.service';
import { StatCategory } from '../contracts/stat-category';
import { SummaryStat } from '../contracts/summary-stat';
import { Team } from '../contracts/team';
import { Park } from '../contracts/park';
import { AsyncPipe } from '@angular/common';
import { GamesComponent } from '../games/games.component';
import { SummaryStatsCardComponent } from '../util-components/summary-stats-card/summary-stats-card.component';

@Component({
    selector: 'app-park',
    standalone: true,
    imports: [
        AsyncPipe,
        GamesComponent,
        SummaryStatsCardComponent
    ],
    templateUrl: './park.component.html',
    styleUrl: './park.component.scss'
})
export class ParkComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.PARK>("parkId")
    public parkId$!: Observable<number>
    park$?: Observable<Park>
    summaryStats$?: Observable<SummaryStat[]>
    get generalStatCategory(): StatCategory {
        return StatCategory.general;
    }

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.park$ = this.parkId$.pipe(switchMap(parkId => {
            return this.api.makeApiGet<Park>(`park/${parkId}`, true, false);
        }));
        this.summaryStats$ = this.parkId$.pipe(switchMap(parkId => {
            return this.api.makeApiGet<SummaryStat[]>('games/summary-stats', { parkId: parkId });
        }))
    }

    cardTitle(park: Park): string {
        return park.name;
    }
}
