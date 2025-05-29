import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FacilityService } from '../../services/facility.service';
import { GoogleMapsModule } from '@angular/google-maps';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { ResolveUrlResponse, Facility } from '../../models/facility.model';

@Component({
  selector: 'app-facility-creation',
  templateUrl: './facility-creation.component.html',
  styleUrls: ['./facility-creation.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, GoogleMapsModule, HttpClientModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class FacilityCreationComponent implements OnInit {
  facilityForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  center: google.maps.LatLngLiteral = { lat: 10.769444, lng: 106.681944 };
  zoom = 12;
  markers: google.maps.LatLngLiteral[] = [
    { lat: 10.769444, lng: 106.681944 }
  ];
  processingUrl = false;
  
  // Edit mode properties
  isEditMode = false;
  facilityId: number | null = null;
  originalFacility: Facility | null = null;

  constructor(
    private fb: FormBuilder,
    private facilityService: FacilityService,
    private router: Router,
    private route: ActivatedRoute,
    private http: HttpClient
  ) {
    this.facilityForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      address: ['', [Validators.required, Validators.maxLength(500)]],
      phoneNumber: ['', [Validators.required, Validators.maxLength(20)]],
      description: [''],
      mapsUrl: [''],
      latitude: [''],
      longitude: [''],
      placeId: ['']
    });
  }

  ngOnInit(): void {
    // Check if we're in edit mode
    this.route.queryParams.subscribe(params => {
      if (params['facilityId']) {
        this.facilityId = +params['facilityId'];
        this.isEditMode = true;
        this.loadFacilityForEdit();
      }
    });
  }

  async loadFacilityForEdit(): Promise<void> {
    if (!this.facilityId) return;
    
    this.isLoading = true;
    this.error = null;
    
    try {
      const facility = await this.facilityService.getFacility(this.facilityId).toPromise();
      this.originalFacility = facility || null;
      
      if (this.originalFacility) {
        // Populate form with existing data
        this.facilityForm.patchValue({
          name: this.originalFacility.name,
          address: this.originalFacility.address,
          phoneNumber: this.originalFacility.phoneNumber,
          description: this.originalFacility.description || '',
          mapsUrl: this.originalFacility.mapsUrl || '',
          latitude: this.originalFacility.courtLatitude || '',
          longitude: this.originalFacility.courtLongitude || '',
          placeId: this.originalFacility.placeId || ''
        });
        
        // Update map if coordinates exist
        if (this.originalFacility.courtLatitude && this.originalFacility.courtLongitude) {
          const lat = parseFloat(this.originalFacility.courtLatitude);
          const lng = parseFloat(this.originalFacility.courtLongitude);
          
          this.center = { lat, lng };
          this.markers = [{ lat, lng }];
          this.zoom = 15;
        }
      }
    } catch (err: any) {
      this.error = err.error?.message || 'Failed to load facility data';
      console.error('Error loading facility:', err);
    } finally {
      this.isLoading = false;
    }
  }

  async processMapUrl() {
    const mapsUrl = this.facilityForm.get('mapsUrl')?.value;
    if (!mapsUrl) return;

    this.processingUrl = true;
    this.error = null;

    try {
      // First, follow any redirects to get the final URL and extract place details
      const response = await this.facilityService.resolveUrl(mapsUrl).toPromise();
      
      if (response) {
        // Update form with place details if available
        const updates: any = {};
        
        if (response.latitude && response.longitude) {
          updates.latitude = response.latitude.toString();
          updates.longitude = response.longitude.toString();
          
          // Update map
          this.center = {
            lat: response.latitude,
            lng: response.longitude
          };
          this.markers = [{
            lat: response.latitude,
            lng: response.longitude
          }];
          this.zoom = 15;
        }
        
        // Update address field if available
        if (response.formattedAddress) {
          updates.address = response.formattedAddress;
        }
        
        // Update facility name if available and name field is empty
        if (response.name && !this.facilityForm.get('name')?.value) {
          updates.name = response.name;
        }
        
        // Update phone number if available
        if (response.phoneNumber) {
          updates.phoneNumber = response.phoneNumber;
        }
        
        // Update placeId if available
        if (response.placeId) {
          updates.placeId = response.placeId;
        }
        
        // Update form values
        this.facilityForm.patchValue(updates);
      } else {
        this.error = 'Could not extract location data from the URL';
      }
    } catch (err) {
      this.error = 'Failed to process the Maps URL';
      console.error(err);
    } finally {
      this.processingUrl = false;
    }
  }

  async onSubmit() {
    if (this.facilityForm.valid) {
      this.isLoading = true;
      this.error = null;

      try {
        const formData = this.facilityForm.value;
        const facilityData = {
          name: formData.name,
          address: formData.address,
          phoneNumber: formData.phoneNumber,
          description: formData.description,
          mapsUrl: formData.mapsUrl,
          courtLatitude: formData.latitude,
          courtLongitude: formData.longitude,
          placeId: formData.placeId
        };
        
        if (this.isEditMode && this.facilityId) {
          // Update existing facility
          await this.facilityService.updateFacility(this.facilityId, facilityData).toPromise();
          // Navigate back to facility overview
          this.router.navigate(['/facility/overview']);
        } else {
          // Create new facility
        const facility = await this.facilityService.createFacility(facilityData).toPromise();
        // Navigate to court creation with the new facility ID
        this.router.navigate(['court/create'], { queryParams: { facilityId: facility?.id }});
        }
      } catch (err: any) {
        this.error = err.error?.message || `Failed to ${this.isEditMode ? 'update' : 'create'} facility`;
      } finally {
        this.isLoading = false;
      }
    }
  }

  onCancel(): void {
    if (this.isEditMode) {
      this.router.navigate(['/facility/overview']);
    } else {
      this.router.navigate(['/facility/overview']);
    }
  }

  get pageTitle(): string {
    return this.isEditMode ? 'Edit Facility' : 'Create New Facility';
  }

  get submitButtonText(): string {
    return this.isEditMode ? 'Update Facility' : 'Create Facility';
  }
} 