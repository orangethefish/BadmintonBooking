import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Subject } from 'rxjs';
import { Router } from '@angular/router';

import { SignupComponent } from './signup.component';
import { AuthService } from '../../services/auth.service';
import { AuthResult, RegisterRequest } from '../../models/auth.model';

// Mock AuthService
class MockAuthService {
  private registerResultSource = new Subject<AuthResult>();
  registerResult$ = this.registerResultSource.asObservable();

  register(username: string, email: string, password: string, accountType: string) {
    return this.registerResult$;
  }

  simulateRegistrationSuccess(result: AuthResult) {
    this.registerResultSource.next(result);
  }

  simulateRegistrationFailure(error: AuthResult) {
    this.registerResultSource.next(error);
  }

  simulateRegistrationHttpError(error: any) {
    this.registerResultSource.error(error);
  }
  // We can keep the login methods if AuthService is expected to have them, 
  // but they are not used by SignupComponent tests directly.
  private loginResultSource = new Subject<AuthResult>();
  loginResult$ = this.loginResultSource.asObservable();
  login(email: string, password: string) { return this.loginResult$; }
}

describe('SignupComponent', () => {
  let component: SignupComponent;
  let fixture: ComponentFixture<SignupComponent>;
  let mockAuthService: MockAuthService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        SignupComponent, // Standalone
        ReactiveFormsModule,
        RouterTestingModule.withRoutes([{ path: 'dashboard', redirectTo: '' }]),
        HttpClientTestingModule
      ],
      providers: [
        FormBuilder,
        { provide: AuthService, useClass: MockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SignupComponent);
    component = fixture.componentInstance;
    mockAuthService = TestBed.inject(AuthService) as unknown as MockAuthService;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize the signup form with required fields', () => {
    expect(component.signupForm).toBeDefined();
    expect(component.signupForm.get('username')).toBeDefined();
    expect(component.signupForm.get('email')).toBeDefined();
    expect(component.signupForm.get('password')).toBeDefined();
    expect(component.signupForm.get('accountType')).toBeDefined();
    expect(component.signupForm.get('accountType')?.value).toBe('User'); // Default value
  });

  it('signup form should be invalid when empty', () => {
    expect(component.signupForm.valid).toBeFalse();
  });

  // Add more granular form validation tests similar to LoginComponent
  it('username field validity', () => {
    let username = component.signupForm.get('username');
    expect(username?.valid).toBeFalse(); // Required
    username?.setValue('testuser');
    expect(username?.valid).toBeTrue();
  });

  it('email field validity', () => {
    let email = component.signupForm.get('email');
    expect(email?.valid).toBeFalse(); // Required
    email?.setValue('test');
    expect(email?.hasError('email')).toBeTrue();
    email?.setValue('test@example.com');
    expect(email?.valid).toBeTrue();
  });

  it('password field validity', () => {
    let password = component.signupForm.get('password');
    expect(password?.valid).toBeFalse(); // Required
    password?.setValue('123');
    expect(password?.hasError('minlength')).toBeTrue();
    password?.setValue('password123');
    expect(password?.valid).toBeTrue();
  });

  it('accountType field validity', () => {
    let accountType = component.signupForm.get('accountType');
    expect(accountType?.valid).toBeTrue(); // Has default and is required
    accountType?.setValue(''); // Set to empty to check required validator
    expect(accountType?.hasError('required')).toBeTrue();
    accountType?.setValue('Admin');
    expect(accountType?.valid).toBeTrue();
  });

  it('signup form should be valid with correct inputs', () => {
    component.signupForm.get('username')?.setValue('testuser');
    component.signupForm.get('email')?.setValue('test@example.com');
    component.signupForm.get('password')?.setValue('password123');
    component.signupForm.get('accountType')?.setValue('User');
    expect(component.signupForm.valid).toBeTrue();
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      spyOn(mockAuthService, 'register').and.callThrough();
      spyOn(router, 'navigate');
    });

    it('should not call authService.register if form is invalid', () => {
      component.onSubmit();
      expect(mockAuthService.register).not.toHaveBeenCalled();
    });

    it('should call authService.register and set isLoading to true if form is valid', () => {
      component.signupForm.patchValue({
        username: 'testuser',
        email: 'test@example.com',
        password: 'password123',
        accountType: 'User'
      });
      component.onSubmit();
      expect(component.isLoading).toBeTrue();
      expect(mockAuthService.register).toHaveBeenCalledWith('testuser', 'test@example.com', 'password123', 'User');
    });

    it('should navigate to /dashboard and reset isLoading on successful registration', () => {
      component.signupForm.patchValue({ username: 'u', email: 'e@e.c', password: 'p123456', accountType: 'User' });
      component.onSubmit();
      const successResult: AuthResult = { success: true, token: 'fake-token', username: 'u', roles: ['User'] };
      mockAuthService.simulateRegistrationSuccess(successResult);
      fixture.detectChanges();
      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBe('');
    });

    it('should set errorMessage and reset isLoading on failed registration (result.success is false)', () => {
      component.signupForm.patchValue({ username: 'u', email: 'e@e.c', password: 'p123456', accountType: 'User' });
      component.onSubmit();
      const failureResult: AuthResult = { success: false, token: '', username: '', roles: [], error: 'Email taken' };
      mockAuthService.simulateRegistrationFailure(failureResult);
      fixture.detectChanges();
      expect(router.navigate).not.toHaveBeenCalled();
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBe('Email taken');
    });

    it('should set default errorMessage if result.error is not provided on failed registration', () => {
      component.signupForm.patchValue({ username: 'u', email: 'e@e.c', password: 'p123456', accountType: 'User' });
      component.onSubmit();
      const failureResult: AuthResult = { success: false, token: '', username: '', roles: [] };
      mockAuthService.simulateRegistrationFailure(failureResult);
      fixture.detectChanges();
      expect(component.errorMessage).toBe('Registration failed');
      expect(component.isLoading).toBeFalse();
    });

    it('should set errorMessage and reset isLoading on http error during registration', () => {
      component.signupForm.patchValue({ username: 'u', email: 'e@e.c', password: 'p123456', accountType: 'User' });
      component.onSubmit();
      const errorResponse = { error: { error: 'Server error' }, message: 'Http failure' };
      mockAuthService.simulateRegistrationHttpError(errorResponse);
      fixture.detectChanges();
      expect(router.navigate).not.toHaveBeenCalled();
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBe('Server error');
    });

    it('should use error.message if error.error.error is not available on http error', () => {
      component.signupForm.patchValue({ username: 'u', email: 'e@e.c', password: 'p123456', accountType: 'User' });
      component.onSubmit();
      const errorResponse = { message: 'Network issue' };
      mockAuthService.simulateRegistrationHttpError(errorResponse);
      fixture.detectChanges();
      expect(component.errorMessage).toBe('Network issue');
    });

    it('should use default message if no specific message is available on http error', () => {
      component.signupForm.patchValue({ username: 'u', email: 'e@e.c', password: 'p123456', accountType: 'User' });
      component.onSubmit();
      const errorResponse = {};
      mockAuthService.simulateRegistrationHttpError(errorResponse);
      fixture.detectChanges();
      expect(component.errorMessage).toBe('An error occurred during registration');
    });
  });
}); 