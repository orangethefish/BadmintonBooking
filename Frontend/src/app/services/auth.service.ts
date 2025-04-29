import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of } from 'rxjs';
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

  register(username: string, email: string, password: string): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/register`, { username, email, password })
      .pipe(
        tap(response => {
          localStorage.setItem('currentUser', JSON.stringify(response));
          this.currentUserSubject.next(response);
        })
      );
  }

  logout(): Observable<any> {
    // Call the backend logout endpoint if user is logged in
    if (this.currentUserValue) {
      return this.http.post<any>(`${environment.apiUrl}/auth/logout`, {}).pipe(
        tap(() => {
          localStorage.removeItem('currentUser');
          this.currentUserSubject.next(null);
        })
      );
    }
    
    // If no user is logged in, just clear local storage
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    return new Observable(observer => {
      observer.next({ message: 'Logged out successfully' });
      observer.complete();
    });
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
}
