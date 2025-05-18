import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of, catchError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResult, LoginRequest, RegisterRequest, User } from '../models/auth.model';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<AuthResult | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  login(email: string, password: string): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(
        tap(response => {
          localStorage.setItem('currentUser', JSON.stringify(response));
          this.currentUserSubject.next(response);
        })
      );
  }

  register(username: string, email: string, password: string, accountType: string = 'User'): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/register`, { 
      username, 
      email, 
      password,
      accountType
    })
      .pipe(
        tap(response => {
          localStorage.setItem('currentUser', JSON.stringify(response));
          this.currentUserSubject.next(response);
        })
      );
  }

  logout(): void {
    // Call the backend logout endpoint if user is logged in
    if (this.currentUserValue) {
      this.http.post<any>(`${environment.apiUrl}/auth/logout`, {}).pipe(
        tap(() => {
          // Clear user data and navigate to login page
          localStorage.removeItem('currentUser');
          this.currentUserSubject.next(null);
          this.router.navigate(['/auth/login']);
        }),
        catchError((error: any) => {
          // Even if the logout API call fails, clear local storage and redirect
          console.error('Logout error:', error);
          localStorage.removeItem('currentUser');
          this.currentUserSubject.next(null);
          this.router.navigate(['/auth/login']);
          return of({ message: 'Logged out successfully' });
        })
      ).subscribe();
    } else {
      // If no user is logged in, just clear local storage and redirect
      localStorage.removeItem('currentUser');
      this.currentUserSubject.next(null);
      this.router.navigate(['/auth/login']);
    }
  }

  isLoggedIn(): boolean {
    const currentUser = this.currentUserSubject.value;
    if (!currentUser || !currentUser.token) {
      return false;
    }
    
    // You could add token expiration check here if your token includes exp claim
    // For now, we'll just check if the token exists
    return true;
  }
  
  getToken(): string {
    const currentUser = this.currentUserSubject.value;
    return currentUser?.token || '';
  }

  get currentUserValue(): AuthResult | null {
    return this.currentUserSubject.value;
  }
  
  hasRole(role: string): boolean {
    const currentUser = this.currentUserSubject.value;
    return currentUser?.roles?.includes(role) || false;
  }
}
