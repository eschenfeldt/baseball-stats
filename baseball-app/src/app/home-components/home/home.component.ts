import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GameCardComponent } from '../game-card/game-card.component';
import { BaseballApiService } from '../../baseball-api.service';
import { Observable } from 'rxjs';
import { GameSummary } from '../../contracts/game-summary';
import { GameDetail } from '../../contracts/game-detail';
import { AsyncPipe } from '@angular/common';
import { PlayerCardComponent } from '../player-card/player-card.component';
import { PlayerSummary } from '../../contracts/player-summary';
import { SummaryStat } from '../../contracts/summary-stat';
import { SummaryStatsComponent } from '../../util-components/summary-stats/summary-stats.component';
import { StatCategory } from '../../contracts/stat-category';
import { SummaryStatsCardComponent } from '../../util-components/summary-stats-card/summary-stats-card.component';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
    standalone: true,
    imports: [
        RouterLink,
        AsyncPipe,
        SummaryStatsCardComponent,
        GameCardComponent,
        PlayerCardComponent
    ]
})
export class HomeComponent implements OnInit {

    public summaryStats$?: Observable<SummaryStat[]>;
    public randomGame$?: Observable<GameSummary>;
    public randomPlayer$?: Observable<PlayerSummary>;

    public constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.summaryStats$ = this.api.makeApiGet<SummaryStat[]>('games/summary-stats');
        this.randomGame$ = this.api.makeApiGet<GameSummary>('games/random');
        this.randomPlayer$ = this.api.makeApiGet<PlayerSummary>('player/random');
    }


}
