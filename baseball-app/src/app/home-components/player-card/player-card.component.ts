import { Component, Input } from '@angular/core';
import { PlayerSummary } from '../../contracts/player-summary';
import { MediaParams } from '../../media-components/media-gallery/media-gallery.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { ThumbnailComponent } from '../../media-components/thumbnail/thumbnail.component';
import { RouterModule } from '@angular/router';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { StatCategory } from '../../contracts/stat-category';
import { PlayerSummaryStatsComponent } from '../../player-components/player-summary-stats/player-summary-stats.component';

@Component({
    selector: 'app-player-card',
    standalone: true,
    imports: [
        MatCardModule,
        MatButtonModule,
        MatTooltipModule,
        MatIconModule,
        RouterModule,
        ThumbnailComponent,
        PlayerSummaryStatsComponent
    ],
    templateUrl: './player-card.component.html',
    styleUrl: './player-card.component.scss'
})
export class PlayerCardComponent {

    @Input()
    purpose?: string;

    @Input({ required: true })
    player!: PlayerSummary;

    get mediaParams(): MediaParams {
        return {
            playerId: this.player.info.id
        };
    }

    get orderedCategories(): StatCategory[] {
        return this.player.summaryStats
            .filter(s => s.value != null && s.definition.name === 'Games')
            .sort((a, b) => b.value! - a.value!) // descending sort
            .map(s => s.category);
    }

    get noGames(): boolean {
        return this.orderedCategories.length === 0;
    }

    hasStatCategory(category: StatCategory): boolean {
        return this.player.summaryStats.some(s => s.category === category);
    }
}
