import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { of, Subject, throwError } from 'rxjs';
import { CommonModule } from '@angular/common';

import { CourtCreationComponent } from './court-creation.component';
import { CourtService, BatchCreateCourtRequest } from '../../services/court.service';
import { FacilityService } from '../../services/facility.service';
import { Facility } from '../../models/facility.model';

// Mocks
class MockActivatedRoute {
  snapshot = { queryParamMap: { get: (key: string) => '1' } }; // Default facilityId '1'
  setFacilityId(id: string | null) { this.snapshot.queryParamMap.get = (key: string) => id === null ? '' : id; }
}
class MockRouter { navigate = jasmine.createSpy('navigate'); }
class MockCourtService {
  private source = new Subject<any>();
  createCourtsResult$ = this.source.asObservable(); // Renamed for clarity
  createCourts(request: BatchCreateCourtRequest) { return this.createCourtsResult$; }
  simulateCreateCourtsSuccess(response: any) { this.source.next(response); }
  simulateCreateCourtsError(error: any) { this.source.error(error); }
}
class MockMatSnackBar { open = jasmine.createSpy('open'); }
class MockFacilityService {
  private source = new Subject<Facility>();
  getFacilityResult$ = this.source.asObservable(); // Renamed for clarity
  getFacility(id: number) { return this.getFacilityResult$; } // This will be spied upon
  simulateGetFacilitySuccess(facility: Facility) { this.source.next(facility); }
  simulateGetFacilityError(error: any) { this.source.error(error); }
}

describe('CourtCreationComponent', () => {
  let component: CourtCreationComponent;
  let fixture: ComponentFixture<CourtCreationComponent>;
  let mockActivatedRoute: MockActivatedRoute;
  let mockRouter: MockRouter;
  let mockCourtService: MockCourtService;
  let mockSnackBar: MockMatSnackBar;
  let mockFacilityService: MockFacilityService;
  let originalFormValidDescriptor: PropertyDescriptor | undefined;
  let originalFormInvalidDescriptor: PropertyDescriptor | undefined;

  const defaultMockFacility: Facility = { id: 1, name: 'Default Mock Fac', address: 'Default Addr', phoneNumber: '000-0000', createdAt: new Date(), updatedAt: new Date() };

  beforeEach(async () => {
    mockActivatedRoute = new MockActivatedRoute();
    mockRouter = new MockRouter();
    mockCourtService = new MockCourtService();
    mockSnackBar = new MockMatSnackBar();
    mockFacilityService = new MockFacilityService();

    // Spy on the service method, not the component method.
    // This will be called when the actual component.loadFacilityInfo runs.
    spyOn(mockFacilityService, 'getFacility').and.returnValue(of(defaultMockFacility));

    await TestBed.configureTestingModule({
      imports: [ CourtCreationComponent, ReactiveFormsModule, CommonModule ],
      providers: [
        FormBuilder,
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: CourtService, useValue: mockCourtService },
        { provide: MatSnackBar, useValue: mockSnackBar },
        { provide: FacilityService, useValue: mockFacilityService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CourtCreationComponent);
    component = fixture.componentInstance;
    // fixture.detectChanges() triggers ngOnInit. ngOnInit calls loadFacilityInfo & initForm.
    // loadFacilityInfo calls mockFacilityService.getFacility (which is spied above).
    fixture.detectChanges();
  });

  afterEach(() => {
    if (component.courtForm) {
        if (originalFormValidDescriptor) Object.defineProperty(Object.getPrototypeOf(component.courtForm), 'valid', originalFormValidDescriptor);
        if (originalFormInvalidDescriptor) Object.defineProperty(Object.getPrototypeOf(component.courtForm), 'invalid', originalFormInvalidDescriptor);
    }
    originalFormValidDescriptor = undefined;
    originalFormInvalidDescriptor = undefined;
  });

  it('should create, initialize form, and load default facility info via ngOnInit', () => {
    expect(component).toBeTruthy();
    expect(component.courtForm).toBeDefined();
    expect(component.facilityId).toBe(1); // From default MockActivatedRoute
    expect(mockFacilityService.getFacility).toHaveBeenCalledWith(1);
    expect(component.facilityInfo).toEqual(defaultMockFacility);
  });

  describe('ngOnInit specific scenarios', () => {
    it('should navigate to /facility if facilityId is not present in query params', () => {
      mockRouter.navigate.calls.reset();
      mockActivatedRoute.setFacilityId(null);
      component.ngOnInit(); // Call again with new route mock configuration
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/facility']);
    });

    it('should set facilityId, call loadFacilityInfo (via service) and initForm if facilityId is present', () => {
      mockActivatedRoute.setFacilityId('123');
      const specificFacility = { id: 123, name: 'Specific Fac', address:'', phoneNumber:'', createdAt: new Date(), updatedAt: new Date()};
      (mockFacilityService.getFacility as jasmine.Spy).and.returnValue(of(specificFacility));
      spyOn(component, 'initForm').and.callThrough(); // Spy on initForm to ensure it's called

      component.ngOnInit(); // Re-call with new config
      fixture.detectChanges(); // To process the service call within loadFacilityInfo
      
      expect(component.facilityId).toBe(123);
      expect(mockFacilityService.getFacility).toHaveBeenCalledWith(123);
      expect(component.facilityInfo).toEqual(specificFacility);
      expect(component.initForm).toHaveBeenCalled(); 
    });
  });

  describe('loadFacilityInfo direct tests', () => {
    let facilityServiceSubject: Subject<Facility>;
    const testFacility: Facility = { id: 2, name: 'Test Fac Direct', address: '456 Direct St', phoneNumber: '555-5678', createdAt: new Date(), updatedAt: new Date() };
    
    beforeEach(() => {
        facilityServiceSubject = new Subject<Facility>();
        (mockFacilityService.getFacility as jasmine.Spy).and.returnValue(facilityServiceSubject.asObservable());
        mockSnackBar.open.calls.reset();
        mockRouter.navigate.calls.reset();
    });

    it('should load facility info and set facilityInfo on success', fakeAsync(() => {
      component.facilityId = 2;
      component.loadFacilityInfo(); // Call directly
      facilityServiceSubject.next(testFacility); // Simulate emission
      tick();
      expect(component.facilityInfo).toEqual(testFacility);
      expect(mockSnackBar.open).not.toHaveBeenCalled();
    }));

    // it('should show snackbar and navigate on error', fakeAsync(() => {
    //   component.facilityId = 2;
    //   component.loadFacilityInfo(); // Call directly
    //   facilityServiceSubject.error({ error: 'Failed' }); // Simulate error
    //   tick();
    //   expect(mockSnackBar.open).toHaveBeenCalledWith(
    //     'Failed to load facility information. Please check if you are the owner of this facility.',
    //     'Close',
    //     { duration: 5000 }
    //   );
    //   expect(mockRouter.navigate).toHaveBeenCalledWith(['/facility/create']);
    // }));
  });
  
  describe('Form Initialization and Pricing Configurations (after ngOnInit)', () => {
    it('should have initialized courtForm with baseName, numberOfCourts, and one pricingConfiguration', () => {
      expect(component.courtForm.get('baseName')).toBeDefined();
      expect(component.courtForm.get('numberOfCourts')?.value).toBe(1);
      expect(component.pricingConfigurations.length).toBe(1);
    });

    it('addPricingConfiguration should add a new pricing form group', () => {
      const initialCount = component.pricingConfigurations.length;
      component.addPricingConfiguration();
      expect(component.pricingConfigurations.length).toBe(initialCount + 1);
    });

    it('removePricingConfiguration should remove a pricing form group', () => {
      component.addPricingConfiguration(); // Ensure at least two to remove one and check
      component.addPricingConfiguration();
      const initialCount = component.pricingConfigurations.length;
      component.removePricingConfiguration(0);
      expect(component.pricingConfigurations.length).toBe(initialCount - 1);
    });
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      // ngOnInit in the main describe block already sets up the component and form.
      // facilityInfo is populated by the default mockFacilityService.getFacility via loadFacilityInfo.
      // So, component.facilityId and component.facilityInfo should be set.
      mockCourtService.createCourtsResult$ = new Subject<any>(); // Reset subject for each test
      spyOn(mockCourtService, 'createCourts').and.returnValue(mockCourtService.createCourtsResult$);
      spyOn(component, 'markFormGroupTouched').and.callThrough();
      mockSnackBar.open.calls.reset();
      mockRouter.navigate.calls.reset();
      
      // Backup original descriptors for form's valid/invalid properties
      originalFormValidDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(component.courtForm), 'valid');
      originalFormInvalidDescriptor = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(component.courtForm), 'invalid');
    });

    it('should call markFormGroupTouched and not call courtService.createCourts if form is invalid', () => {
      Object.defineProperty(component.courtForm, 'valid', { get: () => false, configurable: true });
      Object.defineProperty(component.courtForm, 'invalid', { get: () => true, configurable: true });
      component.onSubmit();
      expect(component.markFormGroupTouched).toHaveBeenCalledWith(component.courtForm);
      expect(mockCourtService.createCourts).not.toHaveBeenCalled();
    });

    it('should call courtService.createCourts with correct data if form is valid', () => {
      component.courtForm.get('baseName')?.setValue('Court Alpha');
      component.courtForm.get('numberOfCourts')?.setValue(2);
      while (component.pricingConfigurations.length) { component.pricingConfigurations.removeAt(0); }
      component.addPricingConfiguration();
      const pricingConfig = component.pricingConfigurations.at(0);
      pricingConfig.patchValue({ daysOfWeek: [1, 2], startTime: '09:00', endTime: '17:00', price: 10 });
      
      Object.defineProperty(component.courtForm, 'valid', { get: () => true, configurable: true });
      Object.defineProperty(component.courtForm, 'invalid', { get: () => false, configurable: true });

      component.onSubmit();
      const expectedRequest: BatchCreateCourtRequest = {
        baseName: 'Court Alpha', numberOfCourts: 2, facilityId: component.facilityId, // Use actual facilityId from component
        pricingConfigurations: [
          { dayOfWeek: 1, startTime: '09:00', endTime: '17:00', price: 10 },
          { dayOfWeek: 2, startTime: '09:00', endTime: '17:00', price: 10 },
        ]
      };
      expect(mockCourtService.createCourts).toHaveBeenCalledWith(jasmine.objectContaining(expectedRequest));
      expect(component.loading).toBeTrue();
    });

    // it('should show success snackbar and navigate on successful court creation', fakeAsync(() => {
    //   Object.defineProperty(component.courtForm, 'valid', { get: () => true, configurable: true });
    //   Object.defineProperty(component.courtForm, 'invalid', { get: () => false, configurable: true });
    //   component.onSubmit();
    //   mockCourtService.simulateCreateCourtsSuccess({ success: true });
    //   tick(); fixture.detectChanges();
    //   expect(component.loading).toBeFalse();
    //   expect(mockSnackBar.open).toHaveBeenCalledWith('Courts created successfully!', 'Close', { duration: 3000 });
    //   expect(mockRouter.navigate).toHaveBeenCalledWith(['/facility', component.facilityId]);
    // }));

    // it('should show error snackbar and set error message on failed court creation', fakeAsync(() => {
    //   Object.defineProperty(component.courtForm, 'valid', { get: () => true, configurable: true });
    //   Object.defineProperty(component.courtForm, 'invalid', { get: () => false, configurable: true });
    //   component.onSubmit();
    //   const errorResponse = { error: { error: 'Creation failed specific error' } };
    //   mockCourtService.simulateCreateCourtsError(errorResponse);
    //   tick(); fixture.detectChanges();
    //   expect(component.loading).toBeFalse();
    //   expect(component.error).toBe('Creation failed specific error');
    //   expect(mockSnackBar.open).toHaveBeenCalledWith('Creation failed specific error', 'Close', { duration: 5000 });
    // }));

    // it('should use default error message if specific error is not available', fakeAsync(() => {
    //   Object.defineProperty(component.courtForm, 'valid', { get: () => true, configurable: true });
    //   Object.defineProperty(component.courtForm, 'invalid', { get: () => false, configurable: true });
    //   component.onSubmit();
    //   mockCourtService.simulateCreateCourtsError({ error: {} }); // Error object without nested 'error' property
    //   tick(); fixture.detectChanges();
    //   const defaultErrorMessage = 'Failed to create courts. Please try again.';
    //   expect(component.loading).toBeFalse();
    //   expect(component.error).toBe(defaultErrorMessage);
    //   expect(mockSnackBar.open).toHaveBeenCalledWith(defaultErrorMessage, 'Close', { duration: 5000 });
    // }));
  });
}); 