import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Facility, ResolveUrlResponse } from '../models/facility.model';

@Injectable({
  providedIn: 'root'
})
export class FacilityService {
  private apiUrl = `${environment.apiUrl}/facility`;

  constructor(private http: HttpClient) {}

  createFacility(facility: any): Observable<Facility> {
    return this.http.post<Facility>(this.apiUrl, facility);
  }

  getFacility(id: number): Observable<Facility> {
    return this.http.get<Facility>(`${this.apiUrl}/${id}`);
  }

  updateFacility(id: number, facility: any): Observable<Facility> {
    return this.http.put<Facility>(`${this.apiUrl}/${id}`, facility);
  }

  deleteFacility(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getFacilities(): Observable<Facility[]> {
    return this.http.get<Facility[]>(this.apiUrl);
  }
  
  resolveUrl(url: string): Observable<ResolveUrlResponse> {
    return this.http.post<ResolveUrlResponse>(`${this.apiUrl}/resolve-url`, { url });
  }
} 