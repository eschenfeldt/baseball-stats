import { Component, OnInit } from '@angular/core';
import { param } from '../param.decorator';
import { BASEBALL_ROUTES } from '../app.routes';
import { Observable, switchMap } from 'rxjs';
import { ApiMethod, BaseballApiService } from '../baseball-api.service';
import { Team } from '../contracts/team';
import { AsyncPipe } from '@angular/common';
import { GamesComponent } from '../games/games.component';

@Component({
    selector: 'app-team',
    standalone: true,
    imports: [
        AsyncPipe,
        GamesComponent
    ],
    templateUrl: './team.component.html',
    styleUrl: './team.component.scss'
})
export class TeamComponent implements OnInit {

    @param<typeof BASEBALL_ROUTES.TEAM>("teamId")
    public teamId$!: Observable<number>
    team$?: Observable<Team>

    constructor(
        private api: BaseballApiService
    ) { }

    ngOnInit(): void {
        this.team$ = this.teamId$.pipe(switchMap(teamId => {
            return this.api.makeApiGet<Team>(`teams/${teamId}`, true, false);
        }));
    }
}
