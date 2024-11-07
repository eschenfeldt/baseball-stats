import { Component, Input, OnInit } from '@angular/core';
import { GameSummary } from '../../contracts/game-summary';
import { MatCardModule } from '@angular/material/card';
import { BaseballApiService } from '../../baseball-api.service';
import { RemoteFileDetail } from '../../contracts/remote-file-detail';
import { Observable } from 'rxjs';
import { ThumbnailComponent } from '../../media-components/thumbnail/thumbnail.component';
import { MediaParams } from '../../media-components/media-gallery/media-gallery.component';
import { AsyncPipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { Team } from '../../contracts/team';
import { Utils } from '../../utils';
import { MatDividerModule } from '@angular/material/divider';

@Component({
    selector: 'app-game-card',
    standalone: true,
    imports: [
        MatButtonModule,
        MatCardModule,
        MatIconModule,
        MatTooltipModule,
        MatDividerModule,
        ThumbnailComponent,
        AsyncPipe,
        RouterModule
    ],
    templateUrl: './game-card.component.html',
    styleUrl: './game-card.component.scss'
})
export class GameCardComponent implements OnInit {

    @Input()
    purpose?: string;

    @Input({ required: true })
    game!: GameSummary;

    image$?: Observable<RemoteFileDetail>;

    get mediaParams(): MediaParams {
        return {
            gameId: this.game.id
        }
    }

    public constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.image$ = this.api.makeApiGet<RemoteFileDetail>('media/random', { gameId: this.game.id, size: 'large' });
    }

    public color(team: Team): string {
        return Utils.teamColorOrDefault(team);
    }

    formatTime(datetime?: string): string {
        return Utils.formatTime(datetime);
    }
}
