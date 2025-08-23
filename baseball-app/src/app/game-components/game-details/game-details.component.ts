import { Component, Input } from '@angular/core';
import { GameDetail } from '../../contracts/game-detail';
import { MatDividerModule } from '@angular/material/divider';
import { RouterModule } from '@angular/router';
import { Utils } from '../../utils';

@Component({
    selector: 'app-game-details',
    imports: [
        RouterModule,
        MatDividerModule
    ],
    templateUrl: './game-details.component.html',
    styleUrl: './game-details.component.scss'
})
export class GameDetailsComponent {

    @Input({ required: true })
    game!: GameDetail

    formatTime(datetime?: string): string {
        return Utils.formatTime(datetime);
    }
}
