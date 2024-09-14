import { Component, Input } from '@angular/core';
import { PlayerSummary } from '../contracts/player-summary';
import { MatCardModule } from '@angular/material/card';
import { StatPipe } from '../stat.pipe';

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
}
