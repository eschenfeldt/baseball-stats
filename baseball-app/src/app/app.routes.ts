import { Routes } from '@angular/router';
import { AdminViewComponent } from './admin/admin.component';
import { HomeComponent } from './home/home.component';

export const routes: Routes = [
    { path: 'home', component: HomeComponent },
    { path: 'admin', component: AdminViewComponent }
];
