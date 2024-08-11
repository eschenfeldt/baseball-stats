import { Component, Input } from '@angular/core';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { GameDetail } from '../contracts/game-detail';

@Component({
    selector: 'app-scorecard',
    standalone: true,
    imports: [
        PdfViewerModule
    ],
    templateUrl: './scorecard.component.html',
    styleUrl: './scorecard.component.scss'
})
export class ScorecardComponent {

    @Input({ required: true })
    game!: GameDetail;

    get url(): string {
        return 'https://eschenfeldt-baseball-media.nyc3.cdn.digitaloceanspaces.com/scorecards/scorecard.pdf';
    }

    get urlWithOrigin(): string {
        return `${this.url}?origin=${window.location.host}`;
    }
}
