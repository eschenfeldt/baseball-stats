import { Component } from '@angular/core';
import { LeaderboardBattersComponent } from '../leaderboard-batters/leaderboard-batters.component';

@Component({
    selector: 'app-leaders',
    standalone: true,
    imports: [LeaderboardBattersComponent],
    templateUrl: './leaders.component.html',
    styleUrl: './leaders.component.scss'
})
export class LeadersComponent {

}
