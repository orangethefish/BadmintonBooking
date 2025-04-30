import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/home/home.routes').then(m => m.HOME_ROUTES)
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'facility',
    loadChildren: () => import('./features/facility/facility.routes').then(m => m.FACILITY_ROUTES)
  },
  {
    path: 'court',
    loadChildren: () => import('./features/court/court.routes').then(m => m.COURT_ROUTES)
  },
  // {
  //   path: 'booking',
  //   loadChildren: () => import('./features/booking/booking.routes').then(m => m.BOOKING_ROUTES)
  // },
  {
    path: '**',
    redirectTo: ''
  }
];
