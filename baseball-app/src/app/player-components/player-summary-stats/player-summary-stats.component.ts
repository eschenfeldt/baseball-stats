import { Component, Input } from '@angular/core';
import { PlayerSummary } from '../../contracts/player-summary';
import { MatCardModule } from '@angular/material/card';
import { StatPipe } from '../../stat.pipe';
import { Utils } from '../../utils';
import { SummaryStat } from '../../contracts/summary-stat';
import { StatCategory } from '../../contracts/stat-category';

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

    @Input({ required: true })
    category!: StatCategory

    public get categoryLabel(): string {
        switch (this.category) {
            case StatCategory.batting:
                return 'Batting';
            case StatCategory.pitching:
                return 'Pitching'
            case StatCategory.fielding:
                return 'Fielding'
            default:
                return '';
        }
    }

    public get stats() {
        return this.player.summaryStats.filter(s => s.category === this.category);
    }
    public get showIP(): boolean {
        return this.category === StatCategory.pitching && this.fullInningsPitched != null;
    }
    public get fullInningsPitched(): string | null {
        return Utils.fullSummaryInningsPitched(this.player.summaryStats);
    }
    public get partialInningsPitched(): string | null {
        return Utils.partialSummaryInningsPitched(this.player.summaryStats);
    }

}
