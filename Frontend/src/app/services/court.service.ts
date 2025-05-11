import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Court, PricingConfigurationRequest } from '../models/court.model';

export interface CreateCourtRequest {
  name: string;
  facilityId: number;
  pricingConfigurations: PricingConfigurationRequest[];
}

export interface BatchCreateCourtRequest {
  baseName: string;
  numberOfCourts: number;
  facilityId: number;
  pricingConfigurations: PricingConfigurationRequest[];
}

@Injectable({
  providedIn: 'root'
})
export class CourtService {
  private apiUrl = `${environment.apiUrl}/court`;

  constructor(private http: HttpClient) {}

  getCourts(facilityId: number): Observable<Court[]> {
    return this.http.get<Court[]>(`${this.apiUrl}?facilityId=${facilityId}`);
  }

  createCourt(request: CreateCourtRequest): Observable<Court> {
    console.log('Sending court request:', request);
    return this.http.post<Court>(this.apiUrl, request).pipe(
      tap({
        error: (error) => console.error('Error creating court:', error)
      })
    );
  }

  createBatchCourts(request: BatchCreateCourtRequest): Observable<Court[]> {
    console.log('Sending batch request:', request);
    return this.http.post<Court[]>(`${this.apiUrl}/batch`, request).pipe(
      tap({
        error: (error) => console.error('Error creating batch courts:', error)
      })
    );
  }

  updateCourt(id: number, court: Partial<Court>): Observable<Court> {
    return this.http.put<Court>(`${this.apiUrl}/${id}`, court);
  }

  deleteCourt(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 