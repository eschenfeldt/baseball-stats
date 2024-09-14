import { Component, Input } from '@angular/core';
import { PlayerSummary } from '../contracts/player-summary';
import { MatCardModule } from '@angular/material/card';
import { StatPipe } from '../stat.pipe';
import { Utils } from '../utils';

@Component({
    selector: 'app-player-summary-stats',
    standalone: true,
    imports: [
        MatCardModule,
        StatPipe
    ],
    templateUrl: './player-summary-stats.component.html',
    styleUrl: './player-summary-stats.component.scss'
})
export class PlayerSummaryStatsComponent {

    @Input({ required: true })
    player!: PlayerSummary

    public get stats() {
        return this.player.summaryStats.filter(s => s.definition.name !== 'ThirdInningsPitched');
    }
    public get fullInningsPitched(): string | null {
        return Utils.fullSummaryInningsPitched(this.player.summaryStats);
    }
    public get partialInningsPitched(): string | null {
        return Utils.partialSummaryInningsPitched(this.player.summaryStats);
    }
}
