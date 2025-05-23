import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FacilityService } from './facility.service';
import { Facility, ResolveUrlResponse } from '../models/facility.model';
import { environment } from '../../environments/environment';

describe('FacilityService', () => {
  let service: FacilityService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/facility`;

  const mockFacility: Facility = {
    id: 1,
    name: 'Test Facility',
    address: '123 Test St',
    phoneNumber: '555-1234',
    createdAt: new Date(),
    updatedAt: new Date()
  };

  const mockFacilities: Facility[] = [mockFacility];

  const mockResolveUrlResponse: ResolveUrlResponse = {
    finalUrl: 'http://example.com/resolved',
    name: 'Resolved Name'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [FacilityService]
    });
    service = TestBed.inject(FacilityService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('createFacility should make a POST request', () => {
    const newFacilityData = { name: 'New Facility' };
    service.createFacility(newFacilityData).subscribe(response => {
      expect(response).toEqual(mockFacility);
    });
    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newFacilityData);
    req.flush(mockFacility);
  });

  it('getFacility should make a GET request', () => {
    const facilityId = 1;
    service.getFacility(facilityId).subscribe(response => {
      expect(response).toEqual(mockFacility);
    });
    const req = httpMock.expectOne(`${apiUrl}/${facilityId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockFacility);
  });

  it('updateFacility should make a PUT request', () => {
    const facilityId = 1;
    const updatedFacilityData = { name: 'Updated Facility' };
    service.updateFacility(facilityId, updatedFacilityData).subscribe(response => {
      expect(response).toEqual(mockFacility); // Assuming backend returns the facility
    });
    const req = httpMock.expectOne(`${apiUrl}/${facilityId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updatedFacilityData);
    req.flush(mockFacility);
  });

  it('deleteFacility should make a DELETE request', () => {
    const facilityId = 1;
    service.deleteFacility(facilityId).subscribe(response => {
      expect(response).toBeTruthy(); // Or whatever the expected response is
    });
    const req = httpMock.expectOne(`${apiUrl}/${facilityId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true }); // Example response
  });

  it('getFacilities should make a GET request for an array', () => {
    service.getFacilities().subscribe(response => {
      expect(response).toEqual(mockFacilities);
    });
    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('GET');
    req.flush(mockFacilities);
  });

  it('resolveUrl should make a POST request', () => {
    const urlToResolve = 'http://example.com';
    service.resolveUrl(urlToResolve).subscribe(response => {
      expect(response).toEqual(mockResolveUrlResponse);
    });
    const req = httpMock.expectOne(`${apiUrl}/resolve-url`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ url: urlToResolve });
    req.flush(mockResolveUrlResponse);
  });

}); 