import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FacilityService {
  private apiUrl = `${environment.apiUrl}/facility`;

  constructor(private http: HttpClient) {}

  createFacility(facility: any): Observable<any> {
    return this.http.post(this.apiUrl, facility);
  }

  getFacility(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  updateFacility(id: number, facility: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, facility);
  }

  deleteFacility(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getFacilities(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }
} 