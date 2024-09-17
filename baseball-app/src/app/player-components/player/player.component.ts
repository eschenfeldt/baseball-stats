import { Component, OnInit, ViewChild } from '@angular/core';
import { BaseballApiService } from '../../baseball-api.service';
import { first, Observable, switchMap } from 'rxjs';
import { Player } from '../../contracts/player';
import { RouterModule } from '@angular/router';
import { param } from '../../param.decorator';
import { BASEBALL_ROUTES } from '../../app.routes';
import { AsyncPipe } from '@angular/common';
import { PlayerBattingStatsComponent } from '../player-batting-stats/player-batting-stats.component';
import { PlayerSummary } from '../../contracts/player-summary';
import { MatCardModule } from '@angular/material/card';
import { PlayerSummaryStatsComponent } from '../player-summary-stats/player-summary-stats.component';
import { PlayerGamesComponent } from '../player-games/player-games.component';

@Component({
    selector: 'app-player',
    standalone: true,
    imports: [
        AsyncPipe,
        RouterModule,
        PlayerSummaryStatsComponent,
        PlayerBattingStatsComponent,
        PlayerGamesComponent
    ],
    templateUrl: './player.component.html',
    styleUrl: './player.component.scss'
})
export class PlayerComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.PLAYER>('playerId')
    playerId$!: Observable<number>
    player$?: Observable<PlayerSummary>;

    @ViewChild(PlayerGamesComponent) gamesComponent?: PlayerGamesComponent;

    public get gamesIdentifier(): string | undefined {
        if (this.gamesComponent) {
            return this.gamesComponent.dataSource.uniqueIdentifier;
        } else {
            return undefined;
        }
    }

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
