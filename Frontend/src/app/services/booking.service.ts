import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BookingRequest {
  courtId: number;
  startTime: Date;
  endTime: Date;
  notes?: string;
}

export interface BookingResponse {
  id: number;
  courtId: number;
  courtName: string;
  facilityName: string;
  startTime: Date;
  endTime: Date;
  totalPrice: number;
  status: string;
  notes?: string;
  createdAt: Date;
  updatedAt?: Date;
}

export interface TimeSlot {
  startTime: Date;
  endTime: Date;
  isAvailable: boolean;
  price: number;
}

export interface CourtAvailabilityResponse {
  courtId: number;
  courtName: string;
  facilityName: string;
  date: Date;
  timeSlots: TimeSlot[];
}

export interface BookingLockRequest {
  courtId: number;
  startTime: Date;
  endTime: Date;
}

export interface BookingLockResponse {
  id: number;
  courtId: number;
  startTime: Date;
  endTime: Date;
  expiresAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private apiUrl = `${environment.apiUrl}/booking`;

  constructor(private http: HttpClient) { }

  // Get all bookings for the current user
  getUserBookings(): Observable<BookingResponse[]> {
    return this.http.get<BookingResponse[]>(this.apiUrl);
  }

  // Get a specific booking by ID
  getBooking(id: number): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(`${this.apiUrl}/${id}`);
  }

  // Create a new booking
  createBooking(booking: BookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(this.apiUrl, booking);
  }

  // Update an existing booking
  updateBooking(id: number, booking: BookingRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, booking);
  }

  // Cancel a booking
  cancelBooking(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Get court availability for a specific date
  getCourtAvailability(courtId: number, date: Date): Observable<CourtAvailabilityResponse> {
    const formattedDate = date.toISOString().split('T')[0];
    return this.http.get<CourtAvailabilityResponse>(
      `${this.apiUrl}/court/${courtId}/availability?date=${formattedDate}`
    );
  }

  // Create a booking lock (to prevent double bookings during the booking process)
  createBookingLock(lock: BookingLockRequest): Observable<BookingLockResponse> {
    return this.http.post<BookingLockResponse>(`${this.apiUrl}/lock`, lock);
  }

  // Release a booking lock
  releaseBookingLock(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/lock/${id}`);
  }
}
