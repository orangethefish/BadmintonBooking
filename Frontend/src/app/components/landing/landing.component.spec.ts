import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BehaviorSubject } from 'rxjs';

import { LandingComponent } from './landing.component';
import { AuthService } from '../../services/auth.service';
import { AuthResult } from '../../models/auth.model';

// Mock AuthService (can be shared or re-defined if specific behaviors are needed)
class MockAuthService {
  currentUser$ = new BehaviorSubject<AuthResult | null>(null);
  isLoggedIn(): boolean {
    return !!this.currentUser$.value?.token;
  }
  logout(): void {
    this.currentUser$.next(null);
  }
  // Add any other methods used by LandingComponent if necessary
}

describe('LandingComponent', () => {
  let component: LandingComponent;
  let fixture: ComponentFixture<LandingComponent>;
  let authService: MockAuthService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LandingComponent, RouterTestingModule], // LandingComponent is standalone and uses RouterModule/RouterLink
      providers: [
        { provide: AuthService, useClass: MockAuthService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LandingComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as unknown as MockAuthService;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have authService injected', () => {
    expect(component.authService).toBeTruthy();
    expect(component.authService).toBeInstanceOf(MockAuthService);
  });

  it('should call authService.logout when logout is called', () => {
    spyOn(authService, 'logout').and.callThrough();
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
  });
}); 