import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const router = inject(Router);
  
  // Get the token from localStorage
  const currentUser = localStorage.getItem('currentUser');
  let token = '';
  
  if (currentUser) {
    try {
      const userObj = JSON.parse(currentUser);
      token = userObj.token || '';
    } catch (e) {
      console.error('Error parsing user from localStorage', e);
    }
  }
  
  // Clone the request and add the authorization header if token exists
  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    // Return the cloned request with the token
    return next(authReq).pipe(
      catchError((error) => {
        // Handle 401 Unauthorized errors
        if (error instanceof HttpErrorResponse && error.status === 401) {
          // Clear local storage and redirect to landing page
          localStorage.removeItem('currentUser');
          router.navigate(['/']);
        }
        return throwError(() => error);
      })
    );
  }
  
  // If no token, just forward the original request
  return next(req).pipe(
    catchError((error) => {
      // Handle 401 Unauthorized errors
      if (error instanceof HttpErrorResponse && error.status === 401) {
        // Clear local storage and redirect to landing page
        localStorage.removeItem('currentUser');
        router.navigate(['/']);
      }
      return throwError(() => error);
    })
  );
};
