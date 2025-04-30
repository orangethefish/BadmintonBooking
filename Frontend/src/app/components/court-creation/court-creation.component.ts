import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FacilityService } from '../../services/facility.service';
import { CourtService } from '../../services/court.service';

@Component({
  selector: 'app-court-creation',
  templateUrl: './court-creation.component.html',
  styleUrls: ['./court-creation.component.scss']
})
export class CourtCreationComponent implements OnInit {
  facilityId: number | null = null;
  facility: any = null;
  courts: any[] = [];
  courtForm: FormGroup;
  isLoading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private facilityService: FacilityService,
    private courtService: CourtService
  ) {
    this.courtForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50)]]
    });
  }

  async ngOnInit() {
    const facilityId = this.route.snapshot.queryParams['facilityId'];
    if (!facilityId) {
      this.router.navigate(['/create-facility']);
      return;
    }

    this.facilityId = facilityId;
    try {
      this.facility = await this.facilityService.getFacility(facilityId).toPromise();
      this.courts = await this.courtService.getCourts(facilityId).toPromise();
    } catch (err) {
      this.router.navigate(['/create-facility']);
    }
  }

  async onSubmit() {
    if (this.courtForm.valid && this.facilityId) {
      this.isLoading = true;
      this.error = null;

      try {
        const court = await this.courtService.createCourt({
          ...this.courtForm.value,
          facilityId: this.facilityId
        }).toPromise();
        
        this.courts.push(court);
        this.courtForm.reset();
      } catch (err: any) {
        this.error = err.error?.message || 'Failed to create court';
      } finally {
        this.isLoading = false;
      }
    }
  }

  async onFinish() {
    if (this.courts.length === 0) {
      this.error = 'Please add at least one court';
      return;
    }

    // Navigate to facility management or dashboard
    this.router.navigate(['/facilities']);
  }
} 