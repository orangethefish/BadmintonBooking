import { Routes } from '@angular/router';
import { FacilityCreationComponent } from '../../components/facility-creation/facility-creation.component';
import { FacilityOverviewComponent } from '../../components/facility-overview/facility-overview.component';

export const FACILITY_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'overview',
    pathMatch: 'full'
  },
  {
    path: 'overview',
    component: FacilityOverviewComponent
  },
  {
    path: 'create',
    component: FacilityCreationComponent
  },
  {
    path: 'edit',
    component: FacilityCreationComponent
  }
];
