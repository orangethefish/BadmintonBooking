import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CourtService, CreateCourtRequest, BatchCreateCourtRequest } from '../../services/court.service';
import { FacilityService } from '../../services/facility.service';
import { Facility } from '../../models/facility.model';
import { Court, PricingConfiguration, PricingConfigurationRequest } from '../../models/court.model';

@Component({
  selector: 'app-court-creation',
  templateUrl: './court-creation.component.html',
  styleUrls: ['./court-creation.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class CourtCreationComponent implements OnInit {
  courtForm: FormGroup;
  batchForm: FormGroup;
  facility: Facility | null = null;
  courts: Court[] = [];
  error: string | null = null;
  isLoading = false;
  isBatchMode = false;
  isSimplePricingMode = true;
  daysOfWeek = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private courtService: CourtService,
    private facilityService: FacilityService
  ) {
    this.courtForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      pricingConfigurations: this.fb.array([]),
      simplePricing: this.fb.group({
        startTime: ['', Validators.required],
        endTime: ['', Validators.required],
        price: [0, [Validators.required, Validators.min(0)]],
        hourlyRate: [0, [Validators.required, Validators.min(0)]]
      })
    });

    this.batchForm = this.fb.group({
      baseName: ['', [Validators.required, Validators.maxLength(50)]],
      numberOfCourts: [1, [Validators.required, Validators.min(1), Validators.max(20)]],
      pricingConfigurations: this.fb.array([]),
      simplePricing: this.fb.group({
        startTime: ['', Validators.required],
        endTime: ['', Validators.required],
        price: [0, [Validators.required, Validators.min(0)]],
        hourlyRate: [0, [Validators.required, Validators.min(0)]]
      })
    });
  }

  ngOnInit(): void {
    const facilityId = this.route.snapshot.queryParamMap.get('facilityId');
    if (facilityId) {
      this.loadFacility(parseInt(facilityId, 10));
      this.loadCourts(parseInt(facilityId, 10));
    }
  }

  get pricingConfigurations() {
    return this.courtForm.get('pricingConfigurations') as FormArray;
  }

  get batchPricingConfigurations() {
    return this.batchForm.get('pricingConfigurations') as FormArray;
  }

  get simplePricing() {
    return this.courtForm.get('simplePricing') as FormGroup;
  }

  get batchSimplePricing() {
    return this.batchForm.get('simplePricing') as FormGroup;
  }

  toggleMode(): void {
    this.isBatchMode = !this.isBatchMode;
    this.error = null;
  }

  togglePricingMode(): void {
    this.isSimplePricingMode = !this.isSimplePricingMode;
    if (this.isSimplePricingMode) {
      this.pricingConfigurations.clear();
      this.batchPricingConfigurations.clear();
    }
  }

  addPricingConfiguration(form: FormGroup): void {
    const pricingConfig = this.fb.group({
      selectedDays: [[], [Validators.required, Validators.minLength(1)]],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0)]],
      hourlyRate: [0, [Validators.required, Validators.min(0)]]
    });

    const pricingArray = form.get('pricingConfigurations') as FormArray;
    pricingArray.push(pricingConfig);
  }

  removePricingConfiguration(index: number, form: FormGroup): void {
    const pricingArray = form.get('pricingConfigurations') as FormArray;
    pricingArray.removeAt(index);
  }

  private mapToPricingConfigurations(): PricingConfigurationRequest[] {
    // Map days of week to C# DayOfWeek enum values
    const dayToEnum: { [key: string]: number } = {
      'Sunday': 0,
      'Monday': 1,
      'Tuesday': 2,
      'Wednesday': 3,
      'Thursday': 4,
      'Friday': 5,
      'Saturday': 6
    };

    // Format time and price values for backend
    const formatPricingData = (day: string, timeData: any): PricingConfigurationRequest => {
      // Format time as "hh:mm:ss" string for C# TimeSpan
      const formatTime = (timeStr: string): string => {
        if (!timeStr) return "00:00:00";
        // Ensure seconds are included
        if (timeStr.split(':').length === 2) return `${timeStr}:00`;
        return timeStr;
      };

      return {
        dayOfWeek: dayToEnum[day],
        // Format times for C# TimeSpan parsing
        startTime: formatTime(timeData.startTime),
        endTime: formatTime(timeData.endTime),
        // Ensure numeric values
        price: Number(timeData.price) || 0,
        hourlyRate: Number(timeData.hourlyRate) || 0
      };
    };

    if (this.isSimplePricingMode) {
      const simplePricing = this.isBatchMode ? 
        this.batchForm.get('simplePricing')?.value : 
        this.courtForm.get('simplePricing')?.value;
      
      return this.daysOfWeek.map(day => formatPricingData(day, simplePricing));
    } else {
      const selectedDays = this.courtForm.get('selectedDays')?.value || [];
      const advancedPricing = this.courtForm.get('advancedPricing')?.value;
      
      return selectedDays.map((day: string) => formatPricingData(day, advancedPricing));
    }
  }

  private loadFacility(facilityId: number): void {
    this.facilityService.getFacility(facilityId).subscribe({
      next: (facility) => {
        this.facility = facility;
      },
      error: (error) => {
        this.error = 'Failed to load facility details';
        console.error('Error loading facility:', error);
      }
    });
  }

  private loadCourts(facilityId: number): void {
    this.courtService.getCourts(facilityId).subscribe({
      next: (courts) => {
        this.courts = courts;
      },
      error: (error) => {
        this.error = 'Failed to load courts';
        console.error('Error loading courts:', error);
      }
    });
  }

  onSubmit(): void {
    if (this.isBatchMode) {
      this.submitBatchCourts();
    } else {
      this.submitSingleCourt();
    }
  }

  submitSingleCourt() {
    if (this.courtForm.invalid || !this.facility) {
      return;
    }

    this.isLoading = true;
    this.error = '';

    const formValue = this.courtForm.value;
    const pricingConfigurations = this.mapToPricingConfigurations();

    const request: CreateCourtRequest = {
      name: formValue.name,
      facilityId: this.facility.id,
      pricingConfigurations: pricingConfigurations
    };

    this.courtService.createCourt(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.router.navigate(['/facilities', this.facility?.id]);
      },
      error: (error) => {
        this.isLoading = false;
        this.error = error.error?.message || 'Failed to create court';
      }
    });
  }

  submitBatchCourts() {
    if (this.batchForm.invalid || !this.facility) {
      return;
    }

    this.isLoading = true;
    this.error = '';

    const formValue = this.batchForm.value;
    const pricingConfigurations = this.mapToPricingConfigurations();

    const request: BatchCreateCourtRequest = {
      baseName: formValue.baseName,
      numberOfCourts: formValue.numberOfCourts,
      facilityId: this.facility.id,
      pricingConfigurations: pricingConfigurations
    };

    this.courtService.createBatchCourts(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.router.navigate(['/facilities', this.facility?.id]);
      },
      error: (error) => {
        this.isLoading = false;
        this.error = error.error?.message || 'Failed to create courts';
      }
    });
  }

  onFinish(): void {
    if (this.facility) {
      this.router.navigate(['/facilities', this.facility.id]);
    } else {
      this.router.navigate(['/facilities']);
    }
  }
} 