import { Component, OnInit } from '@angular/core';
import { BaseballApiService } from '../../baseball-api.service';
import { Observable, switchMap } from 'rxjs';
import { RouterModule } from '@angular/router';
import { param } from '../../param.decorator';
import { BASEBALL_ROUTES } from '../../app.routes';
import { AsyncPipe } from '@angular/common';
import { PlayerBattingStatsComponent } from '../player-batting-stats/player-batting-stats.component';
import { PlayerSummary } from '../../contracts/player-summary';
import { SummaryStatsComponent } from '../../util-components/summary-stats/summary-stats.component';
import { PlayerGamesComponent } from '../player-games/player-games.component';
import { PlayerPitchingStatsComponent } from '../player-pitching-stats/player-pitching-stats.component';
import { StatCategory } from '../../contracts/stat-category';

@Component({
    selector: 'app-player',
    standalone: true,
    imports: [
        AsyncPipe,
        RouterModule,
        SummaryStatsComponent,
        PlayerBattingStatsComponent,
        PlayerPitchingStatsComponent,
        PlayerGamesComponent
    ],
    templateUrl: './player.component.html',
    styleUrl: './player.component.scss'
})
export class PlayerComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.PLAYER>('playerId')
    playerId$!: Observable<number>
    player$?: Observable<PlayerSummary>;

    gamesIdentifier?: string;
    battingStatsIdentifier?: string;
    pitchingStatsIdentifier?: string;

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.player$ = this.playerId$.pipe(
            switchMap((playerId) => {
                return this.api.makeApiGet<PlayerSummary>(`player/${playerId}`);
            }));
    }

    setGamesIdentifier(value: string): void {
        this.gamesIdentifier = value;
    }

    setBattingStatsIdentifier(value: string): void {
        this.battingStatsIdentifier = value;
    }

    setPitchingStatsIdentifier(value: string): void {
        this.pitchingStatsIdentifier = value;
    }

    hasStatCategory(player: PlayerSummary, category: StatCategory): boolean {
        return player.summaryStats.some(s => s.category === category);
    }

    orderedCategories(player: PlayerSummary): StatCategory[] {
        return player.summaryStats
            .filter(s => s.value != null && s.definition.name === 'Games')
            .sort((a, b) => b.value! - a.value!) // descending sort
            .map(s => s.category);
    }

    get StatCategory() {
        return StatCategory;
    }
}
