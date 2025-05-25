import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule, AbstractControl, ValidatorFn } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CourtService } from '../../services/court.service';
import { Court, PricingConfiguration } from '../../models/court.model';

export interface CourtEditDialogData {
  court: Court;
  facilityId: number;
}

export interface PricingConfigurationFormData {
  daysOfWeek: number[];
  startTime: string;
  endTime: string;
  price: number;
}

@Component({
  selector: 'app-court-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './court-edit-dialog.component.html',
  styleUrls: ['./court-edit-dialog.component.scss']
})
export class CourtEditDialogComponent implements OnInit {
  courtForm: FormGroup;
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
    private courtService: CourtService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<CourtEditDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CourtEditDialogData
  ) {
    this.courtForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]],
      pricingConfigurations: this.fb.array([], [this.validateNoOverlappingTime(), this.validateAtLeastOnePricing()])
    });
  }

  ngOnInit(): void {
    this.loadCourtData();
  }

  loadCourtData(): void {
    // Set court name
    this.courtForm.patchValue({
      name: this.data.court.name
    });

    // Load existing pricing configurations
    if (this.data.court.pricingConfigurations && this.data.court.pricingConfigurations.length > 0) {
      this.loadExistingPricingConfigurations();
    } else {
      // Add one empty pricing configuration if none exist
      this.addPricingConfiguration();
    }
  }

  loadExistingPricingConfigurations(): void {
    // Group existing pricing configurations by time slots
    const grouped = this.groupPricingByTimeSlot(this.data.court.pricingConfigurations || []);
    
    grouped.forEach(group => {
      const pricingForm = this.fb.group({
        daysOfWeek: [group.daysOfWeek, [this.validateDaysOfWeek()]],
        startTime: [group.startTime, [Validators.required, Validators.pattern(/^([01]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$/)]],
        endTime: [group.endTime, [Validators.required, Validators.pattern(/^([01]?[0-9]|2[0-4]):[0-5][0-9](:[0-5][0-9])?$/)]],
        price: [group.price, [Validators.required, Validators.min(0)]]
      }, { validators: this.validateTimeRange() });
      
      this.pricingConfigurations.push(pricingForm);
    });
  }

  groupPricingByTimeSlot(pricingConfigurations: PricingConfiguration[]): PricingConfigurationFormData[] {
    const grouped: { [key: string]: PricingConfigurationFormData } = {};
    
    pricingConfigurations.forEach(config => {
      const key = `${config.startTime}-${config.endTime}-${config.price}`;
      if (!grouped[key]) {
        grouped[key] = {
          daysOfWeek: [],
          startTime: config.startTime,
          endTime: config.endTime,
          price: config.price
        };
      }
      grouped[key].daysOfWeek.push(parseInt(config.dayOfWeek));
    });

    return Object.values(grouped);
  }

  get pricingConfigurations(): FormArray {
    return this.courtForm.get('pricingConfigurations') as FormArray;
  }

  addPricingConfiguration(): void {
    const pricingForm = this.fb.group({
      daysOfWeek: [[], [this.validateDaysOfWeek()]],
      startTime: ['', [Validators.required, Validators.pattern(/^([01]?[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$/)]],
      endTime: ['', [Validators.required, Validators.pattern(/^([01]?[0-9]|2[0-4]):[0-5][0-9](:[0-5][0-9])?$/)]],
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
      
      // Normalize time format (remove seconds if present)
      const normalizeTime = (time: string): string => {
        const parts = time.split(':');
        return `${parts[0]}:${parts[1]}`;
      };
      
      const normalizedStartTime = normalizeTime(startTime);
      const normalizedEndTime = normalizeTime(endTime);
      
      // Special case for 24:00 end time
      if (normalizedEndTime === '24:00') {
        return normalizedStartTime === '24:00' ? { 'invalidTimeRange': true } : null;
      }
      
      const startParts = normalizedStartTime.split(':');
      const endParts = normalizedEndTime.split(':');
      
      if (startParts.length !== 2 || endParts.length !== 2) {
        return null;
      }
      
      const startDate = new Date();
      startDate.setHours(parseInt(startParts[0]), parseInt(startParts[1]), 0, 0);
      
      const endDate = new Date();
      endDate.setHours(parseInt(endParts[0]), parseInt(endParts[1]), 0, 0);
      
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
      
      // Normalize time format (remove seconds if present)
      const normalizeTime = (time: string): string => {
        const parts = time.split(':');
        return `${parts[0]}:${parts[1]}`;
      };
      
      // Collect all time ranges
      for (let i = 0; i < pricingArray.length; i++) {
        const item = pricingArray.at(i);
        const daysOfWeek = item.get('daysOfWeek')?.value || [];
        const startTime = item.get('startTime')?.value;
        const endTime = item.get('endTime')?.value;
        
        if (!daysOfWeek.length || !startTime || !endTime) {
          continue;
        }
        
        const normalizedStartTime = normalizeTime(startTime);
        const normalizedEndTime = normalizeTime(endTime);
        
        // Special handling for 24:00 end time
        let endTimeForComparison = normalizedEndTime;
        if (normalizedEndTime === '24:00') {
          endTimeForComparison = '23:59';
        }
        
        const startParts = normalizedStartTime.split(':');
        const endParts = endTimeForComparison.split(':');
        
        if (startParts.length !== 2 || endParts.length !== 2) {
          continue;
        }
        
        const startDate = new Date();
        startDate.setHours(parseInt(startParts[0]), parseInt(startParts[1]), 0, 0);
        
        const endDate = new Date();
        endDate.setHours(parseInt(endParts[0]), parseInt(endParts[1]), 0, 0);
        if (normalizedEndTime === '24:00') {
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
      
      return null;
    };
  }

  validateDaysOfWeek(): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      const value = control.value;
      if (!value || !Array.isArray(value) || value.length === 0) {
        return { 'required': true };
      }
      return null;
    };
  }

  validateAtLeastOnePricing(): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      const pricingArray = control as FormArray;
      if (pricingArray.length === 0) {
        return { 'atLeastOnePricing': true };
      }
      return null;
    };
  }

  isDaySelected(configIndex: number, dayValue: number): boolean {
    const daysArray = this.pricingConfigurations.at(configIndex).get('daysOfWeek')?.value || [];
    return daysArray.includes(dayValue);
  }

  toggleDaySelection(configIndex: number, dayValue: number): void {
    const control = this.pricingConfigurations.at(configIndex).get('daysOfWeek');
    if (!control) return;
    
    const currentValue = [...(control.value || [])];
    const index = currentValue.indexOf(dayValue);
    
    if (index === -1) {
      currentValue.push(dayValue);
    } else {
      currentValue.splice(index, 1);
    }
    
    control.setValue(currentValue);
    control.markAsTouched();
  }

  onSave(): void {
    if (this.courtForm.invalid) {
      this.markFormGroupTouched(this.courtForm);
      
      // Debug: Log validation errors
      console.log('Form is invalid. Errors:', this.getFormValidationErrors());
      return;
    }

    this.loading = true;
    this.error = null;

    const formValue = this.courtForm.value;
    
    // Prepare update data
    const updateData = {
      name: formValue.name,
      pricingConfigurations: this.flattenPricingConfigurations(formValue.pricingConfigurations)
    };

    // Call the court service to update
    this.courtService.updateCourtWithPricing(this.data.court.id, updateData).subscribe({
      next: (updatedCourt) => {
        this.loading = false;
        this.snackBar.open('Court updated successfully!', 'Close', { duration: 3000 });
        this.dialogRef.close({
          success: true,
          data: updatedCourt
        });
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.error || 'Failed to update court. Please try again.';
        console.error('Error updating court:', err);
      }
    });
  }

  flattenPricingConfigurations(pricingConfigs: PricingConfigurationFormData[]): any[] {
    const flattened: any[] = [];
    
    // Helper function to normalize time format
    const normalizeTime = (time: string): string => {
      if (!time) return time;
      const parts = time.split(':');
      return `${parts[0]}:${parts[1]}`;
    };
    
    pricingConfigs.forEach(config => {
      config.daysOfWeek.forEach(day => {
        const normalizedStartTime = normalizeTime(config.startTime);
        const normalizedEndTime = normalizeTime(config.endTime);
        
        flattened.push({
          dayOfWeek: day,
          startTime: normalizedStartTime,
          endTime: normalizedEndTime === '24:00' ? '23:59' : normalizedEndTime,
          price: config.price,
          hourlyRate: config.price
        });
      });
    });
    
    return flattened;
  }

  onCancel(): void {
    this.dialogRef.close({ success: false });
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

  // Debug helper method
  getFormValidationErrors(): any {
    const errors: any = {};
    
    // Check main form errors
    if (this.courtForm.errors) {
      errors.form = this.courtForm.errors;
    }
    
    // Check individual field errors
    Object.keys(this.courtForm.controls).forEach(key => {
      const control = this.courtForm.get(key);
      if (control && control.errors) {
        errors[key] = control.errors;
      }
    });
    
    // Check pricing configurations errors
    const pricingArray = this.pricingConfigurations;
    if (pricingArray.errors) {
      errors.pricingConfigurations = pricingArray.errors;
    }
    
    // Check individual pricing configuration errors
    pricingArray.controls.forEach((control, index) => {
      if (control.errors) {
        errors[`pricingConfig_${index}`] = control.errors;
      }
      
      // Check individual fields within pricing config
      Object.keys(control.value).forEach(fieldKey => {
        const fieldControl = control.get(fieldKey);
        if (fieldControl && fieldControl.errors) {
          errors[`pricingConfig_${index}_${fieldKey}`] = fieldControl.errors;
        }
      });
    });
    
    return errors;
  }

  debugValidation(): void {
    console.log('=== FORM VALIDATION DEBUG ===');
    console.log('Form valid:', this.courtForm.valid);
    console.log('Form status:', this.courtForm.status);
    console.log('Form errors:', this.courtForm.errors);
    console.log('Form value:', this.courtForm.value);
    
    // Force validation
    this.markFormGroupTouched(this.courtForm);
    this.courtForm.updateValueAndValidity();
    
    console.log('After force validation:');
    console.log('Form valid:', this.courtForm.valid);
    console.log('Form errors:', this.courtForm.errors);
    console.log('Detailed errors:', this.getFormValidationErrors());
  }
} 