import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CourtService {
  private apiUrl = `${environment.apiUrl}/courts`;

  constructor(private http: HttpClient) {}

  createCourt(court: any): Observable<any> {
    return this.http.post(this.apiUrl, court);
  }

  getCourt(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  updateCourt(id: number, court: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, court);
  }

  deleteCourt(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getCourts(facilityId: number): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/facilities/${facilityId}/courts`);
  }
} 