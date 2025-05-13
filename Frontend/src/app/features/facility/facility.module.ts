import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { FACILITY_ROUTES } from './facility.routes';
import { FacilityCreationComponent } from '../../components/facility-creation/facility-creation.component';

@NgModule({
  declarations: [
    FacilityCreationComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(FACILITY_ROUTES)
  ]
})
export class FacilityModule { }
