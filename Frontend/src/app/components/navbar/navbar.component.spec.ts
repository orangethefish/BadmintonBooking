import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BehaviorSubject } from 'rxjs';

import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../services/auth.service';
import { AuthResult } from '../../models/auth.model';

// Mock AuthService
class MockAuthService {
  currentUser$ = new BehaviorSubject<AuthResult | null>(null);
  isLoggedIn(): boolean {
    return !!this.currentUser$.value?.token;
  }
  logout(): void {
    this.currentUser$.next(null);
  }
}

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let authService: MockAuthService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavbarComponent, RouterTestingModule], // NavbarComponent is standalone
      providers: [
        { provide: AuthService, useClass: MockAuthService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as unknown as MockAuthService; // Get the mock instance
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize isLoggedIn to false when user is not logged in', () => {
    expect(component.isLoggedIn).toBeFalse();
  });

  it('should set isLoggedIn to true when user is logged in', () => {
    authService.currentUser$.next({ success: true, token: 'fake-token', username: 'testuser', roles: ['User'] });
    fixture.detectChanges(); // Trigger ngOnInit again or direct call to subscription if possible/needed
    // Need to re-trigger the subscription or check how ngOnInit updates isLoggedIn
    component.ngOnInit(); // Re-run ngOnInit to pick up the new auth state
    expect(component.isLoggedIn).toBeTrue();
  });

  it('should set isLoggedIn to false after logout', () => {
    // First, log in
    authService.currentUser$.next({ success: true, token: 'fake-token', username: 'testuser', roles: ['User'] });
    component.ngOnInit();
    expect(component.isLoggedIn).toBeTrue();

    // Then, log out
    component.logout();
    // The component's isLoggedIn should be updated by the subscription or direct call in logout
    // Let's assume logout() in component updates isLoggedIn or the subscription handles it
    fixture.detectChanges(); // Allow subscription to update isLoggedIn
    expect(component.isLoggedIn).toBeFalse();
  });

  it('should call authService.logout when logout is called', () => {
    spyOn(authService, 'logout').and.callThrough();
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
  });
}); 