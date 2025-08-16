import { Component, Input } from '@angular/core';
import { SummaryStatsComponent } from '../summary-stats/summary-stats.component';
import { SummaryStat } from '../../contracts/summary-stat';
import { MatCardModule } from '@angular/material/card';
import { StatCategory } from '../../contracts/stat-category';
import { MatExpansionModule } from '@angular/material/expansion';
import { ThumbnailComponent } from '../../media-components/thumbnail/thumbnail.component';
import { RemoteFileDetail } from '../../contracts/remote-file-detail';
import { MediaParams } from '../../media-components/media-gallery/media-gallery.component';

@Component({
    selector: 'app-summary-stats-card',
    standalone: true,
    imports: [
        MatCardModule,
        MatExpansionModule,
        SummaryStatsComponent,
        ThumbnailComponent
    ],
    templateUrl: './summary-stats-card.component.html',
    styleUrl: './summary-stats-card.component.scss'
})
export class SummaryStatsCardComponent {

    @Input({ required: true })
    summaryStats!: SummaryStat[]

    @Input()
    title?: string

    @Input()
    image?: RemoteFileDetail | null

    @Input()
    mediaParams?: MediaParams | null

    public statCategories = [
        StatCategory.general,
        StatCategory.batting,
        StatCategory.pitching
    ];
    hideGames(category: StatCategory): boolean {
        return category !== StatCategory.general;
    }
    hideNull(category: StatCategory): boolean {
        return category === StatCategory.general;
    }
}
