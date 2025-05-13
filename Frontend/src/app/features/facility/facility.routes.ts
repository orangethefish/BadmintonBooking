import { Routes } from '@angular/router';
import { FacilityCreationComponent } from '../../components/facility-creation/facility-creation.component';

export const FACILITY_ROUTES: Routes = [
  {
    path: 'create',
    component: FacilityCreationComponent
  }
];
