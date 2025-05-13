import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, AbstractControl, ValidatorFn } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatOptionModule } from '@angular/material/core';
import { CourtService, BatchCreateCourtRequest } from '../../services/court.service';
import { PricingConfigurationFormData, PricingConfigurationRequest } from '../../models/court.model';
import { Facility } from '../../models/facility.model';
import { FacilityService } from '../../services/facility.service';

@Component({
  selector: 'app-court-creation',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatOptionModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './court-creation.component.html',
  styleUrls: ['./court-creation.component.scss']
})
export class CourtCreationComponent implements OnInit {
  facilityId!: number;
  facilityInfo: Facility | null = null;
  courtForm!: FormGroup;
  loading = false;
  error: string | null = null;
  
  daysOfWeek = [
    { value: 0, name: 'Sunday' },
    { value: 1, name: 'Monday' },
    { value: 2, name: 'Tuesday' },
    { value: 3, name: 'Wednesday' },
    { value: 4, name: 'Thursday' },
    { value: 5, name: 'Friday' },
    { value: 6, name: 'Saturday' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private courtService: CourtService,
    private snackBar: MatSnackBar,
    private facilityService: FacilityService
  ) { }

  ngOnInit(): void {
    this.facilityId = +this.route.snapshot.queryParamMap.get('facilityId')!;
    if (!this.facilityId) {
      this.router.navigate(['/facility']);
      return;
    }
      
    this.loadFacilityInfo();
    this.initForm();
  }

  loadFacilityInfo(): void {
    this.facilityService.getFacility(this.facilityId).subscribe({
      next: (data) => {
        this.facilityInfo = data;
      },
      error: (err) => {
        this.snackBar.open(
          'Failed to load facility information. Please check if you are the owner of this facility.',
          'Close',
          { duration: 5000 }
        );
        this.router.navigate(['/facility/create']);
      }
    });
  }

  initForm(): void {
    this.courtForm = this.fb.group({
      baseName: ['', [Validators.required, Validators.maxLength(50)]],
      numberOfCourts: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
      pricingConfigurations: this.fb.array([], this.validateNoOverlappingTime())
    });
    
    // Add initial pricing configuration
    this.addPricingConfiguration();
  }

  get pricingConfigurations(): FormArray {
    return this.courtForm.get('pricingConfigurations') as FormArray;
  }

  addPricingConfiguration(): void {
    const pricingForm = this.fb.group({
      daysOfWeek: [[], [Validators.required]],
      startTime: ['', [Validators.required, Validators.pattern(/^([01]?[0-9]|2[0-3]):[0-5][0-9]$/)]],
      endTime: ['', [Validators.required, Validators.pattern(/^([01]?[0-9]|2[0-4]):[0-5][0-9]$/)]],
      price: [0, [Validators.required, Validators.min(0)]]
    }, { validators: this.validateTimeRange() });
    
    this.pricingConfigurations.push(pricingForm);
  }

  removePricingConfiguration(index: number): void {
    this.pricingConfigurations.removeAt(index);
  }

  validateTimeRange(): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      const startTime = control.get('startTime')?.value;
      const endTime = control.get('endTime')?.value;
      
      if (!startTime || !endTime) {
        return null;
      }
      
      // Special case for 24:00 end time
      if (endTime === '24:00') {
        // For 24:00, it should be valid as long as start time is not 24:00
        return startTime === '24:00' ? { 'invalidTimeRange': true } : null;
      }
      
      const startParts = startTime.split(':');
      const endParts = endTime.split(':');
      
      if (startParts.length !== 2 || endParts.length !== 2) {
        return null;
      }
      
      const startDate = new Date();
      startDate.setHours(parseInt(startParts[0]), parseInt(startParts[1]));
      
      const endDate = new Date();
      endDate.setHours(parseInt(endParts[0]), parseInt(endParts[1]));
      
      return startDate >= endDate ? { 'invalidTimeRange': true } : null;
    };
  }

  validateNoOverlappingTime(): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      const pricingArray = control as FormArray;
      
      if (pricingArray.length <= 1) {
        return null;
      }
      
      const timeRanges: { day: number, start: Date, end: Date }[] = [];
      
      // Collect all time ranges
      for (let i = 0; i < pricingArray.length; i++) {
        const item = pricingArray.at(i);
        const daysOfWeek = item.get('daysOfWeek')?.value || [];
        const startTime = item.get('startTime')?.value;
        const endTime = item.get('endTime')?.value;
        
        if (!daysOfWeek.length || !startTime || !endTime) {
          continue;
        }
        
        // Special handling for 24:00 end time
        let endTimeForComparison = endTime;
        if (endTime === '24:00') {
          endTimeForComparison = '23:59';
        }
        
        const startParts = startTime.split(':');
        const endParts = endTimeForComparison.split(':');
        
        if (startParts.length !== 2 || endParts.length !== 2) {
          continue;
        }
        
        const startDate = new Date();
        startDate.setHours(parseInt(startParts[0]), parseInt(startParts[1]));
        
        const endDate = new Date();
        endDate.setHours(parseInt(endParts[0]), parseInt(endParts[1]));
        // If it's 24:00/00:00, set it to 23:59 for comparison but add 1 minute
        if (endTime === '24:00') {
          endDate.setMinutes(endDate.getMinutes() + 1);
        }
        
        for (const day of daysOfWeek) {
          timeRanges.push({ day, start: startDate, end: endDate });
        }
      }
      
      // Check for overlaps
      for (let i = 0; i < timeRanges.length; i++) {
        for (let j = i + 1; j < timeRanges.length; j++) {
          if (timeRanges[i].day === timeRanges[j].day) {
            // Check if time ranges overlap
            if (
              (timeRanges[i].start <= timeRanges[j].start && timeRanges[i].end > timeRanges[j].start) ||
              (timeRanges[i].start < timeRanges[j].end && timeRanges[i].end >= timeRanges[j].end) ||
              (timeRanges[i].start >= timeRanges[j].start && timeRanges[i].end <= timeRanges[j].end)
            ) {
              return { 'overlappingTimes': true };
            }
          }
        }
      }
      
      // Check for gaps
      for (let day = 0; day < 7; day++) {
        const dayRanges = timeRanges
          .filter(r => r.day === day)
          .sort((a, b) => a.start.getTime() - b.start.getTime());
        
        if (dayRanges.length === 0) {
          continue; // No configurations for this day
        }
        
        // First start time should be 00:00
        const firstRange = dayRanges[0];
        const firstTime = firstRange.start.getHours() * 60 + firstRange.start.getMinutes();
        if (firstTime !== 0) {
          return { 'gapInTimes': true };
        }
        
        // Check for gaps between time ranges
        for (let i = 0; i < dayRanges.length - 1; i++) {
          const endTime = dayRanges[i].end.getHours() * 60 + dayRanges[i].end.getMinutes();
          const nextStartTime = dayRanges[i + 1].start.getHours() * 60 + dayRanges[i + 1].start.getMinutes();
          
          if (endTime !== nextStartTime) {
            return { 'gapInTimes': true };
          }
        }
        
        // Last end time should be 24:00 (represented as 23:59+1)
        const lastRange = dayRanges[dayRanges.length - 1];
        const lastTime = lastRange.end.getHours() * 60 + lastRange.end.getMinutes();
        
        // We're looking for 00:00 next day, which we represent as 23:59+1
        if (lastTime !== (23 * 60 + 59 + 1)) {
          return { 'gapInTimes': true };
        }
      }
      
      return null;
    };
  }

  onSubmit(): void {
    if (this.courtForm.invalid) {
      this.markFormGroupTouched(this.courtForm);
      return;
    }
    
    this.loading = true;
    const formValue = this.courtForm.value;
    
    // Create the API request object
    const request: BatchCreateCourtRequest = {
      baseName: formValue.baseName,
      numberOfCourts: formValue.numberOfCourts,
      facilityId: this.facilityId,
      pricingConfigurations: []
    };
    
    // Flatten pricing configurations
    formValue.pricingConfigurations.forEach((config: PricingConfigurationFormData) => {
      config.daysOfWeek.forEach(day => {
        request.pricingConfigurations.push({
          dayOfWeek: day,
          startTime: config.startTime,
          // Convert 24:00 to 23:59 for the backend
          endTime: config.endTime === '24:00' ? '23:59' : config.endTime,
          price: config.price
        });
      });
    });
    
    this.courtService.createCourts(request).subscribe({
      next: (response) => {
        this.loading = false;
        this.snackBar.open('Courts created successfully!', 'Close', { duration: 3000 });
        this.router.navigate(['/facility', this.facilityId]);
      },
      error: (err) => {
        this.loading = false;
        const errorMessage = err.error?.error || 'Failed to create courts. Please try again.';
        this.error = errorMessage;
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
      }
    });
  }
  
  markFormGroupTouched(formGroup: FormGroup | FormArray): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup || control instanceof FormArray) {
        this.markFormGroupTouched(control);
      } else if (control) {
        control.markAsTouched();
      }
    });
  }
}
