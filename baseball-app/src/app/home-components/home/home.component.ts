import { Component, OnInit } from '@angular/core';
import { GameCardComponent } from '../game-card/game-card.component';
import { BaseballApiService } from '../../baseball-api.service';
import { Observable } from 'rxjs';
import { GameSummary } from '../../contracts/game-summary';
import { AsyncPipe } from '@angular/common';
import { PlayerCardComponent } from '../player-card/player-card.component';
import { PlayerSummary } from '../../contracts/player-summary';
import { SummaryStat } from '../../contracts/summary-stat';
import { SummaryStatsCardComponent } from '../../util-components/summary-stats-card/summary-stats-card.component';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
    imports: [
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
    public gamesOnDate$?: Observable<GameSummary[]>;
    public gamesOnOppositeDate$?: Observable<GameSummary[]>;

    public constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.summaryStats$ = this.api.makeApiGet<SummaryStat[]>('games/summary-stats');
        this.randomGame$ = this.api.makeApiGet<GameSummary>('games/random');
        this.randomPlayer$ = this.api.makeApiGet<PlayerSummary>('player/random');
        const date = new Date();
        this.gamesOnDate$ = this.api.makeApiGet<GameSummary[]>('games/on-date', { month: date.getMonth() + 1, day: date.getDate() });
        const oppositeMonth = (date.getMonth() + 6) % 12 + 1;
        this.gamesOnOppositeDate$ = this.api.makeApiGet<GameSummary[]>('games/on-date', { month: oppositeMonth, day: date.getDate() });
    }

    gamePurpose(game: GameSummary, onThisDate: boolean = true) {
        const year = new Date(game.date).getFullYear();
        if (onThisDate) {
            return `On this date in ${year}`;
        } else {
            return `Six months from today's date in ${year}`;
        }
    }
}
