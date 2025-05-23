import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CourtService, BatchCreateCourtRequest } from './court.service';
import { Court } from '../models/court.model';
import { environment } from '../../environments/environment';

describe('CourtService', () => {
  let service: CourtService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/court`;

  const mockCourt: Court = {
    id: 1,
    name: 'Court 1',
    facilityId: 1,
    ownerId: 10,
    isActive: true,
    createdAt: new Date(),
    pricingConfigurations: [] // Simplified for this mock
  };

  const mockCourts: Court[] = [mockCourt];

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CourtService]
    });
    service = TestBed.inject(CourtService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getCourts should make a GET request with facilityId query param', () => {
    const facilityId = 100;
    service.getCourts(facilityId).subscribe(response => {
      expect(response).toEqual(mockCourts);
    });
    const req = httpMock.expectOne(`${apiUrl}?facilityId=${facilityId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockCourts);
  });

  it('createCourts (batch) should make a POST request to /batch', () => {
    const batchRequest: BatchCreateCourtRequest = {
      baseName: 'Court',
      numberOfCourts: 3,
      facilityId: 1,
      pricingConfigurations: []
    };
    service.createCourts(batchRequest).subscribe(response => {
      expect(response).toEqual(mockCourts); // Assuming it returns the created courts
    });
    const req = httpMock.expectOne(`${apiUrl}/batch`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(batchRequest);
    req.flush(mockCourts);
  });

  it('updateCourt should make a PUT request', () => {
    const courtId = 1;
    const courtUpdateData: Partial<Court> = { name: 'Updated Court Name' };
    service.updateCourt(courtId, courtUpdateData).subscribe(response => {
      expect(response).toEqual(mockCourt);
    });
    const req = httpMock.expectOne(`${apiUrl}/${courtId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(courtUpdateData);
    req.flush(mockCourt);
  });

  it('deleteCourt should make a DELETE request', () => {
    const courtId = 1;
    service.deleteCourt(courtId).subscribe(); // Observable<void>
    const req = httpMock.expectOne(`${apiUrl}/${courtId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null); // For void response
  });

}); 