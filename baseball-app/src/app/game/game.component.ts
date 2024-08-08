import { Component, OnInit } from '@angular/core';
import { BaseballApiService } from '../baseball-api.service';
import { param } from '../param.decorator';
import { BASEBALL_ROUTES } from '../app.routes';
import { Observable, switchMap } from 'rxjs';
import { GameDetail } from '../contracts/game-detail';
import { AsyncPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GameBatter } from '../contracts/game-batter';
import { BoxScoreBattersComponent } from '../box-score-batters/box-score-batters.component';

@Component({
    selector: 'app-game',
    standalone: true,
    imports: [
        AsyncPipe,
        MatCardModule,
        MatButtonToggleModule,
        FormsModule,
        ReactiveFormsModule,
        BoxScoreBattersComponent
    ],
    templateUrl: './game.component.html',
    styleUrl: './game.component.scss'
})
export class GameComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.GAME>('gameId')
    gameId$!: Observable<number>
    game$?: Observable<GameDetail>;

    boxScoreOption: BoxScoreOption = BoxScoreOption.homeBatters;

    public battersDataSource(game: GameDetail): GameBatter[] | null {
        switch (this.boxScoreOption) {
            case BoxScoreOption.homeBatters:
                return game.homeBoxScore.batters;
            case BoxScoreOption.awayBatters:
                return game.awayBoxScore.batters;
            default:
                return null;
        }
    }

    constructor(
        private api: BaseballApiService
    ) { }

    public ngOnInit(): void {

        this.game$ = this.gameId$.pipe(
            switchMap((gameId) => {
                return this.api.makeApiGet<GameDetail>(`games/${gameId}`);
            }));
    }
}

enum BoxScoreOption {
    awayBatters = 'awayBatters',
    awayPitchers = 'awayPitchers',
    awayFielders = 'awayFielders',
    homeBatters = 'homeBatters',
    homePitchers = 'homePitchers',
    homeFielders = 'homeFielders'
}
