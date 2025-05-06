import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { FacilityService } from '../../services/facility.service';

@Component({
  selector: 'app-facility-creation',
  templateUrl: './facility-creation.component.html',
  styleUrls: ['./facility-creation.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class FacilityCreationComponent implements OnInit {
  facilityForm: FormGroup;
  isLoading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private facilityService: FacilityService,
    private router: Router
  ) {
    this.facilityForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      address: ['', [Validators.required, Validators.maxLength(500)]],
      phoneNumber: ['', [Validators.required, Validators.maxLength(20)]],
      description: ['']
    });
  }

  ngOnInit(): void {}

  async onSubmit() {
    if (this.facilityForm.valid) {
      this.isLoading = true;
      this.error = null;

      try {
        const facility = await this.facilityService.createFacility(this.facilityForm.value).toPromise();
        // Navigate to court creation with the new facility ID
        this.router.navigate(['court/create'], { queryParams: { facilityId: facility.id }});
      } catch (err: any) {
        this.error = err.error?.message || 'Failed to create facility';
      } finally {
        this.isLoading = false;
      }
    }
  }
} 