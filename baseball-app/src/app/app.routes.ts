import { Routes } from '@angular/router';
import { AdminViewComponent } from './admin/admin.component';
import { HomeComponent } from './home/home.component';
import { GamesComponent } from './games/games.component';
import { LeadersComponent } from './leaders/leaders.component';
import { GameComponent } from './game/game.component';
import { TeamsComponent } from './teams/teams.component';
import { TeamComponent } from './team/team.component';
import { PlayerComponent } from './player/player.component';

export const BASEBALL_ROUTES = <const>{
    HOME: 'home',
    GAMES: 'games',
    GAME: 'game/:gameId',
    TEAMS: 'teams',
    TEAM: 'team/:teamId',
    LEADERS: 'leaders',
    PLAYER: 'player/:playerId',
    ADMIN: 'admin'
}

export const routes: Routes = [
    { path: BASEBALL_ROUTES.HOME, component: HomeComponent },
    { path: BASEBALL_ROUTES.GAMES, component: GamesComponent },
    { path: BASEBALL_ROUTES.GAME, component: GameComponent },
    { path: BASEBALL_ROUTES.TEAMS, component: TeamsComponent },
    { path: BASEBALL_ROUTES.TEAM, component: TeamComponent },
    { path: BASEBALL_ROUTES.LEADERS, component: LeadersComponent },
    { path: BASEBALL_ROUTES.PLAYER, component: PlayerComponent },
    { path: BASEBALL_ROUTES.ADMIN, component: AdminViewComponent },
    { path: '', redirectTo: 'home', pathMatch: 'full' }
];
