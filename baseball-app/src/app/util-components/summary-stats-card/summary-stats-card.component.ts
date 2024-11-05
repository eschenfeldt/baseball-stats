import { Component, Input } from '@angular/core';
import { SummaryStatsComponent } from '../summary-stats/summary-stats.component';
import { SummaryStat } from '../../contracts/summary-stat';
import { MatCardModule } from '@angular/material/card';
import { StatCategory } from '../../contracts/stat-category';

@Component({
    selector: 'app-summary-stats-card',
    standalone: true,
    imports: [
        MatCardModule,
        SummaryStatsComponent
    ],
    templateUrl: './summary-stats-card.component.html',
    styleUrl: './summary-stats-card.component.scss'
})
export class SummaryStatsCardComponent {

    @Input({ required: true })
    summaryStats!: SummaryStat[]

    @Input()
    title?: string

    public statCategories = [
        StatCategory.general,
        StatCategory.batting,
        StatCategory.pitching
    ];
    hideGames(category: StatCategory): boolean {
        return category !== StatCategory.general;
    }
}
