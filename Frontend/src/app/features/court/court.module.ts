import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { COURT_ROUTES } from './court.routes';
import { CourtCreationComponent } from '../../components/court-creation/court-creation.component';

@NgModule({
  declarations: [
    CourtCreationComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(COURT_ROUTES)
  ]
})
export class CourtModule { }
