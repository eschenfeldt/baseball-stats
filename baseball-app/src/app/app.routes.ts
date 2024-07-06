import { Routes } from '@angular/router';
import { AdminViewComponent } from './admin/admin.component';
import { HomeComponent } from './home/home.component';
import { GamesComponent } from './games/games.component';

export const routes: Routes = [
    { path: 'home', component: HomeComponent },
    { path: 'games', component: GamesComponent },
    { path: 'admin', component: AdminViewComponent }
];
