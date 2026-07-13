import { Routes } from '@angular/router';

import { TrackListComponent } from './pages/track-list/track-list';
import { TrackDetailComponent } from './pages/track-detail/track-detail';
import { LoginComponent } from './pages/login/login';

export const routes: Routes = [
  { path: '', component: TrackListComponent },
  { path: 'tracks/:id', component: TrackDetailComponent },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' },
];
