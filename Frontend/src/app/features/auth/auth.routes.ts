import { Routes } from '@angular/router';
import { LoginComponent } from '../../components/auth/login.component';
import { SignupComponent } from '../../components/auth/signup.component';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'signup',
    component: SignupComponent
  }
]; 