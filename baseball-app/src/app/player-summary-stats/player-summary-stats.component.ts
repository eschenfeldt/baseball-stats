import { Component, Input } from '@angular/core';
import { PlayerSummary } from '../contracts/player-summary';
import { MatCardModule } from '@angular/material/card';
import { StatDef } from '../contracts/stat-def';
import { Utils } from '../utils';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-player-summary-stats',
    standalone: true,
    imports: [
        MatCardModule,
        CommonModule
    ],
    templateUrl: './player-summary-stats.component.html',
    styleUrl: './player-summary-stats.component.scss'
})
export class PlayerSummaryStatsComponent {

    @Input({ required: true })
    player!: PlayerSummary

    public formatString(statDef: StatDef): string {
        return Utils.formatString(statDef);
    }
}
