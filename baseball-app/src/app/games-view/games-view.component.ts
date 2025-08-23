import { Component } from '@angular/core';
import { GamesComponent } from '../games/games.component';

@Component({
    selector: 'app-games-view',
    imports: [
        GamesComponent
    ],
    templateUrl: './games-view.component.html',
    styleUrl: './games-view.component.scss'
})
export class GamesViewComponent {

}
