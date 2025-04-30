import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { AUTH_ROUTES } from './auth.routes';
import { LoginComponent } from '../../components/auth/login.component';
import { SignupComponent } from '../../components/auth/signup.component';

@NgModule({
  declarations: [
    LoginComponent,
    SignupComponent
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(AUTH_ROUTES)
  ]
})
export class AuthModule { }
