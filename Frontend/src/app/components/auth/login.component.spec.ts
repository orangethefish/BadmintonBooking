import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError, Subject } from 'rxjs';
import { Router } from '@angular/router';

import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';
import { AuthResult } from '../../models/auth.model';

// Mock AuthService
class MockAuthService {
  // Using a Subject to allow tests to push values
  private loginResultSource = new Subject<AuthResult>();
  loginResult$ = this.loginResultSource.asObservable();

  login(email: string, password: string) {
    // Simulate an API call, actual logic to be controlled by test cases
    return this.loginResult$;
  }

  // Helper to simulate successful login from tests
  simulateLoginSuccess(result: AuthResult) {
    this.loginResultSource.next(result);
  }

  // Helper to simulate login failure from tests
  simulateLoginFailure(error: AuthResult) {
    this.loginResultSource.next(error); // For business logic errors (e.g., wrong password)
  }

  // Helper to simulate HTTP error from tests
  simulateLoginHttpError(error: any) {
    this.loginResultSource.error(error); // For network/server errors
  }
}

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let mockAuthService: MockAuthService;
  let router: Router;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LoginComponent, // Standalone
        ReactiveFormsModule,
        RouterTestingModule.withRoutes([{ path: 'dashboard', redirectTo: '' }]), // For navigation test
        HttpClientTestingModule // LoginComponent imports HttpClientModule
      ],
      providers: [
        FormBuilder, // LoginComponent uses FormBuilder directly
        { provide: AuthService, useClass: MockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    mockAuthService = TestBed.inject(AuthService) as unknown as MockAuthService;
    router = TestBed.inject(Router);
    fixture.detectChanges(); // Initial data binding and ngOnInit if any
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize the login form with email and password controls', () => {
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.get('email')).toBeDefined();
    expect(component.loginForm.get('password')).toBeDefined();
  });

  it('login form should be invalid when empty', () => {
    expect(component.loginForm.valid).toBeFalse();
  });

  it('email field validity', () => {
    let email = component.loginForm.get('email');
    expect(email?.valid).toBeFalse(); // Initially required

    email?.setValue('test');
    expect(email?.hasError('email')).toBeTrue(); // Invalid email format

    email?.setValue('test@example.com');
    expect(email?.valid).toBeTrue();
  });

  it('password field validity', () => {
    let password = component.loginForm.get('password');
    expect(password?.valid).toBeFalse(); // Initially required

    password?.setValue('123');
    expect(password?.hasError('minlength')).toBeTrue(); // Too short

    password?.setValue('password123');
    expect(password?.valid).toBeTrue();
  });

  it('login form should be valid with correct inputs', () => {
    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('password123');
    expect(component.loginForm.valid).toBeTrue();
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      // Spy on authService.login and call through to the mock implementation
      spyOn(mockAuthService, 'login').and.callThrough(); 
      spyOn(router, 'navigate');
    });

    it('should not call authService.login if form is invalid', () => {
      component.onSubmit();
      expect(mockAuthService.login).not.toHaveBeenCalled();
    });

    it('should call authService.login and set isLoading to true if form is valid', () => {
      component.loginForm.get('email')?.setValue('test@example.com');
      component.loginForm.get('password')?.setValue('password123');
      component.onSubmit();
      expect(component.isLoading).toBeTrue();
      expect(mockAuthService.login).toHaveBeenCalledWith('test@example.com', 'password123');
    });

    it('should navigate to /dashboard and reset isLoading on successful login', () => {
      component.loginForm.get('email')?.setValue('test@example.com');
      component.loginForm.get('password')?.setValue('password123');
      component.onSubmit();

      const successResult: AuthResult = { success: true, token: 'fake-token', username: 'user', roles: ['User'] };
      mockAuthService.simulateLoginSuccess(successResult);
      fixture.detectChanges();

      expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBe('');
    });

    it('should set errorMessage and reset isLoading on failed login (result.success is false)', () => {
      component.loginForm.get('email')?.setValue('test@example.com');
      component.loginForm.get('password')?.setValue('password123');
      component.onSubmit();

      const failureResult: AuthResult = { success: false, token: '', username: '', roles: [], error: 'Invalid credentials' };
      mockAuthService.simulateLoginFailure(failureResult);
      fixture.detectChanges();

      expect(router.navigate).not.toHaveBeenCalled();
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBe('Invalid credentials');
    });

    it('should set errorMessage to default if result.error is not provided on failed login', () => {
      component.loginForm.get('email')?.setValue('test@example.com');
      component.loginForm.get('password')?.setValue('password123');
      component.onSubmit();

      const failureResult: AuthResult = { success: false, token: '', username: '', roles: [] }; // No error message
      mockAuthService.simulateLoginFailure(failureResult);
      fixture.detectChanges();

      expect(component.errorMessage).toBe('Login failed');
      expect(component.isLoading).toBeFalse();
    });

    it('should set errorMessage and reset isLoading on http error during login', () => {
      component.loginForm.get('email')?.setValue('test@example.com');
      component.loginForm.get('password')?.setValue('password123');
      component.onSubmit();

      const errorResponse = { error: { error: 'Server unavailable' }, message: 'Http failure' };
      mockAuthService.simulateLoginHttpError(errorResponse);
      fixture.detectChanges();

      expect(router.navigate).not.toHaveBeenCalled();
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBe('Server unavailable');
    });

     it('should use error.message if error.error.error is not available', () => {
      component.loginForm.get('email')?.setValue('test@example.com');
      component.loginForm.get('password')?.setValue('password123');
      component.onSubmit();

      const errorResponse = { message: 'Network error' }; // No nested error.error
      mockAuthService.simulateLoginHttpError(errorResponse);
      fixture.detectChanges();

      expect(component.errorMessage).toBe('Network error');
    });

    it('should use default message if no specific error message is available', () => {
      component.loginForm.get('email')?.setValue('test@example.com');
      component.loginForm.get('password')?.setValue('password123');
      component.onSubmit();

      const errorResponse = {}; // Empty error object
      mockAuthService.simulateLoginHttpError(errorResponse);
      fixture.detectChanges();

      expect(component.errorMessage).toBe('An error occurred during login');
    });
  });
}); 