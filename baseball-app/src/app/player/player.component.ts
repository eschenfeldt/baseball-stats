import { Component, OnInit } from '@angular/core';
import { BaseballApiService } from '../baseball-api.service';
import { first, Observable, switchMap } from 'rxjs';
import { Player } from '../contracts/player';
import { RouterModule } from '@angular/router';
import { param } from '../param.decorator';
import { BASEBALL_ROUTES } from '../app.routes';
import { AsyncPipe } from '@angular/common';
import { PlayerBattingStatsComponent } from '../player-batting-stats/player-batting-stats.component';
import { PlayerSummary } from '../contracts/player-summary';

@Component({
    selector: 'app-player',
    standalone: true,
    imports: [
        AsyncPipe,
        RouterModule,
        PlayerBattingStatsComponent
    ],
    templateUrl: './player.component.html',
    styleUrl: './player.component.scss'
})
export class PlayerComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.PLAYER>('playerId')
    playerId$!: Observable<number>
    player$?: Observable<PlayerSummary>;

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.player$ = this.playerId$.pipe(
            switchMap((playerId) => {
                return this.api.makeApiGet<PlayerSummary>(`player/${playerId}`);
            }));
    }
}
