import { Routes } from '@angular/router';
import { CourtCreationComponent } from '../../components/court-creation/court-creation.component';

export const COURT_ROUTES: Routes = [
  // {
  //   path: '',
  //   component: CourtListComponent
  // },
  {
    path: 'create',
    component: CourtCreationComponent
  },
  // {
  //   path: ':id',
  //   component: CourtDetailComponent
  // }
]; 