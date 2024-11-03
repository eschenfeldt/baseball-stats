import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GameCardComponent } from '../game-card/game-card.component';
import { BaseballApiService } from '../../baseball-api.service';
import { Observable } from 'rxjs';
import { GameSummary } from '../../contracts/game-summary';
import { GameDetail } from '../../contracts/game-detail';
import { AsyncPipe } from '@angular/common';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
    standalone: true,
    imports: [
        RouterLink,
        AsyncPipe,
        GameCardComponent
    ]
})
export class HomeComponent implements OnInit {

    public randomGame$?: Observable<GameSummary>;

    public constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.randomGame$ = this.api.makeApiGet<GameDetail>('games/random');
    }


}
