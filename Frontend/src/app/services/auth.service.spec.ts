import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';

import { AuthService } from './auth.service';
import { AuthResult } from '../models/auth.model';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let router: Router;
  let store: { [key: string]: string } = {};

  const mockAuthResult: AuthResult = {
    success: true,
    token: 'fake-token',
    username: 'testuser',
    roles: ['User']
  };

  beforeEach(() => {
    store = {}; // Reset store for each test
    spyOn(localStorage, 'getItem').and.callFake((key: string) => store[key] || null);
    spyOn(localStorage, 'setItem').and.callFake((key: string, value: string) => store[key] = value);
    spyOn(localStorage, 'removeItem').and.callFake((key: string) => delete store[key]);

    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        RouterTestingModule.withRoutes([{ path: 'auth/login', redirectTo: '' }]) // For logout navigation
      ],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    spyOn(router, 'navigate'); // Spy on router navigation
  });

  afterEach(() => {
    httpMock.verify(); // Make sure that there are no outstanding requests
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('constructor should load user from localStorage if present', () => {
    localStorage.setItem('currentUser', JSON.stringify(mockAuthResult));
    // Service will be re-created by TestBed if we call TestBed.inject again, 
    // or we need to test the state of the already created service if constructor logic runs once.
    // For simplicity, let's create a new instance for this specific test scenario.
    const newService = TestBed.runInInjectionContext(() => new AuthService(TestBed.inject(HttpTestingController) as any, router));
    expect(newService.currentUserValue).toEqual(mockAuthResult);
  });

  describe('login', () => {
    it('should make a POST request to login and update user state on success', () => {
      service.login('test@example.com', 'password').subscribe(response => {
        expect(response).toEqual(mockAuthResult);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email: 'test@example.com', password: 'password' });
      req.flush(mockAuthResult);

      expect(service.currentUserValue).toEqual(mockAuthResult);
      expect(localStorage.getItem('currentUser')).toEqual(JSON.stringify(mockAuthResult));
    });
  });

  describe('register', () => {
    it('should make a POST request to register and update user state on success', () => {
      service.register('newuser', 'new@example.com', 'newpass', 'User').subscribe(response => {
        expect(response).toEqual(mockAuthResult); // Assuming same AuthResult structure for test
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/auth/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ username: 'newuser', email: 'new@example.com', password: 'newpass', accountType: 'User' });
      req.flush(mockAuthResult);

      expect(service.currentUserValue).toEqual(mockAuthResult);
      expect(localStorage.getItem('currentUser')).toEqual(JSON.stringify(mockAuthResult));
    });
  });

  describe('logout', () => {
    it('should clear user state, remove from localStorage, and navigate to login when not logged in', () => {
      // Ensure user is not logged in initially by clearing subject if necessary
      (service as any).currentUserSubject.next(null); // Force logged out state
      store = {}; // Clear localStorage mock

      service.logout();

      expect(service.currentUserValue).toBeNull();
      expect(localStorage.getItem('currentUser')).toBeNull();
      expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
      httpMock.expectNone(`${environment.apiUrl}/auth/logout`); // No HTTP call if not logged in
    });

    it('should make POST to logout, clear user state, and navigate when logged in', () => {
      // Log in user first
      store['currentUser'] = JSON.stringify(mockAuthResult);
      (service as any).currentUserSubject.next(mockAuthResult);

      service.logout();

      const req = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
      expect(req.request.method).toBe('POST');
      req.flush({ message: 'Logged out' }); // Simulate successful backend logout

      expect(service.currentUserValue).toBeNull();
      expect(localStorage.getItem('currentUser')).toBeNull();
      expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
    });

    it('should clear user state and navigate even if backend logout fails', () => {
      store['currentUser'] = JSON.stringify(mockAuthResult);
      (service as any).currentUserSubject.next(mockAuthResult);

      service.logout();

      const req = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
      req.error(new ProgressEvent('error')); // Simulate HTTP error

      expect(service.currentUserValue).toBeNull();
      expect(localStorage.getItem('currentUser')).toBeNull();
      expect(router.navigate).toHaveBeenCalledWith(['/auth/login']);
    });
  });

  describe('isLoggedIn', () => {
    it('should return false if no current user', () => {
      (service as any).currentUserSubject.next(null);
      expect(service.isLoggedIn()).toBeFalse();
    });

    it('should return false if current user has no token', () => {
      (service as any).currentUserSubject.next({ success: true, token: '', username: 'user', roles: [] });
      expect(service.isLoggedIn()).toBeFalse();
    });

    it('should return true if current user has a token', () => {
      (service as any).currentUserSubject.next(mockAuthResult);
      expect(service.isLoggedIn()).toBeTrue();
    });
  });

  describe('getToken', () => {
    it('should return empty string if no current user', () => {
      (service as any).currentUserSubject.next(null);
      expect(service.getToken()).toBe('');
    });

    it('should return token if current user exists', () => {
      (service as any).currentUserSubject.next(mockAuthResult);
      expect(service.getToken()).toBe('fake-token');
    });
  });

  describe('currentUserValue', () => {
    it('should return the current user value', () => {
      (service as any).currentUserSubject.next(mockAuthResult);
      expect(service.currentUserValue).toEqual(mockAuthResult);
      (service as any).currentUserSubject.next(null);
      expect(service.currentUserValue).toBeNull();
    });
  });

  describe('hasRole', () => {
    it('should return false if no current user', () => {
      (service as any).currentUserSubject.next(null);
      expect(service.hasRole('User')).toBeFalse();
    });

    it('should return false if user roles are not defined', () => {
      (service as any).currentUserSubject.next({ success: true, token: 't', username: 'u', roles: undefined as any });
      expect(service.hasRole('User')).toBeFalse();
    });

    it('should return false if user does not have the role', () => {
      (service as any).currentUserSubject.next(mockAuthResult); // Has 'User' role
      expect(service.hasRole('Admin')).toBeFalse();
    });

    it('should return true if user has the role', () => {
      (service as any).currentUserSubject.next(mockAuthResult);
      expect(service.hasRole('User')).toBeTrue();
    });
  });

}); 