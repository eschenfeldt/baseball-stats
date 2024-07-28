import { Routes } from '@angular/router';
import { AdminViewComponent } from './admin/admin.component';
import { HomeComponent } from './home/home.component';
import { GamesComponent } from './games/games.component';
import { LeadersComponent } from './leaders/leaders.component';

export const routes: Routes = [
    { path: 'home', component: HomeComponent },
    { path: 'games', component: GamesComponent },
    { path: 'leaders', component: LeadersComponent },
    { path: 'admin', component: AdminViewComponent }
];
