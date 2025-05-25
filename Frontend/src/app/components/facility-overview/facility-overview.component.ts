import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FacilityService } from '../../services/facility.service';
import { Facility } from '../../models/facility.model';

@Component({
  selector: 'app-facility-overview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './facility-overview.component.html',
  styleUrls: ['./facility-overview.component.scss']
})
export class FacilityOverviewComponent implements OnInit {
  facilities: Facility[] = [];
  loading = false;
  error: string | null = null;
  activeMenuFacilityId: number | null = null;

  constructor(
    private facilityService: FacilityService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadFacilities();
  }

  loadFacilities(): void {
    this.loading = true;
    this.error = null;
    
    this.facilityService.getFacilities().subscribe({
      next: (facilities) => {
        this.facilities = facilities;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load facilities';
        this.loading = false;
        console.error('Error loading facilities:', err);
      }
    });
  }

  onCardClick(facilityId: number): void {
    this.router.navigate(['/court/overview'], { queryParams: { facilityId } });
  }

  onMenuClick(event: Event, facilityId: number): void {
    event.stopPropagation(); // Prevent card click
    this.activeMenuFacilityId = this.activeMenuFacilityId === facilityId ? null : facilityId;
  }

  onViewCourts(facilityId: number): void {
    this.activeMenuFacilityId = null;
    this.router.navigate(['/court/overview'], { queryParams: { facilityId } });
  }

  onEditFacility(facilityId: number): void {
    this.activeMenuFacilityId = null;
    this.router.navigate(['/facility/edit'], { queryParams: { facilityId } });
  }

  onClickOutside(): void {
    this.activeMenuFacilityId = null;
  }

  onCreateFacility(): void {
    this.router.navigate(['/facility/create']);
  }
} 