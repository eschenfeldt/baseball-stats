import { Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { StatPipe } from '../../stat.pipe';
import { Utils } from '../../utils';
import { SummaryStat } from '../../contracts/summary-stat';
import { StatCategory } from '../../contracts/stat-category';

@Component({
    selector: 'app-summary-stats',
    standalone: true,
    imports: [
        MatCardModule,
        StatPipe
    ],
    templateUrl: './summary-stats.component.html',
    styleUrl: './summary-stats.component.scss'
})
export class SummaryStatsComponent {

    @Input({ required: true })
    summaryStats!: SummaryStat[]

    @Input({ required: true })
    category!: StatCategory

    @Input()
    hideGames?: boolean

    @Input()
    hideNull?: boolean

    public get categoryLabel(): string {
        switch (this.category) {
            case StatCategory.batting:
                return 'Batting';
            case StatCategory.pitching:
                return 'Pitching'
            case StatCategory.fielding:
                return 'Fielding'
            case StatCategory.general:
                return 'Overall'
            default:
                return '';
        }
    }

    public get stats() {
        return this.summaryStats.filter(s => s.category === this.category && !(this.hideGames && s.definition.name === 'Games'));
    }
    public get showIP(): boolean {
        return this.category === StatCategory.pitching && this.fullInningsPitched != null;
    }
    public get fullInningsPitched(): string | null {
        return Utils.fullSummaryInningsPitched(this.summaryStats);
    }
    public get partialInningsPitched(): string | null {
        return Utils.partialSummaryInningsPitched(this.summaryStats);
    }

}
