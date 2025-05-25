import { Routes } from '@angular/router';
import { CourtCreationComponent } from '../../components/court-creation/court-creation.component';
import { CourtOverviewComponent } from '../../components/court-overview/court-overview.component';

export const COURT_ROUTES: Routes = [
  {
    path: 'overview',
    component: CourtOverviewComponent
  },
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