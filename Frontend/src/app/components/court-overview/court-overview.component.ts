import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CourtService } from '../../services/court.service';
import { FacilityService } from '../../services/facility.service';
import { Court, PricingConfiguration } from '../../models/court.model';
import { Facility } from '../../models/facility.model';
import { CourtEditDialogComponent, CourtEditDialogData } from '../court-edit-dialog/court-edit-dialog.component';

@Component({
  selector: 'app-court-overview',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './court-overview.component.html',
  styleUrls: ['./court-overview.component.scss']
})
export class CourtOverviewComponent implements OnInit {
  facilityId!: number;
  facility: Facility | null = null;
  courts: Court[] = [];
  loading = false;
  error: string | null = null;
  activeMenuCourtId: number | null = null;

  daysOfWeek = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private courtService: CourtService,
    private facilityService: FacilityService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.facilityId = +params['facilityId'];
      if (this.facilityId) {
        this.loadFacilityInfo();
        this.loadCourts();
      } else {
        this.router.navigate(['/facility/overview']);
      }
    });
  }

  loadFacilityInfo(): void {
    this.facilityService.getFacility(this.facilityId).subscribe({
      next: (facility) => {
        this.facility = facility;
      },
      error: (err) => {
        console.error('Error loading facility:', err);
      }
    });
  }

  loadCourts(): void {
    this.loading = true;
    this.error = null;

    this.courtService.getCourts(this.facilityId).subscribe({
      next: (courts) => {
        this.courts = courts;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load courts';
        this.loading = false;
        console.error('Error loading courts:', err);
      }
    });
  }

  onBackToFacilities(): void {
    this.router.navigate(['/facility/overview']);
  }

  onCreateCourt(): void {
    this.router.navigate(['/court/create'], { queryParams: { facilityId: this.facilityId } });
  }

  onEditCourt(court: Court): void {
    const dialogData: CourtEditDialogData = {
      court: court,
      facilityId: this.facilityId
    };

    const dialogRef = this.dialog.open(CourtEditDialogComponent, {
      width: '800px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && result.success) {
        this.loadCourts(); // Reload courts to show updated data
      }
    });
  }

  onDeleteCourt(court: Court): void {
    if (confirm(`Are you sure you want to delete "${court.name}"? This action cannot be undone.`)) {
      this.courtService.deleteCourt(court.id).subscribe({
        next: () => {
          this.snackBar.open('Court deleted successfully!', 'Close', { duration: 3000 });
          this.loadCourts(); // Reload courts
        },
        error: (err) => {
          const errorMessage = err.error?.error || 'Failed to delete court. Please try again.';
          this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
          console.error('Error deleting court:', err);
        }
      });
    }
  }

  onViewBookings(court: Court): void {
    // TODO: Navigate to bookings view for this court
    this.snackBar.open('Bookings view coming soon!', 'Close', { duration: 2000 });
  }

  groupPricingByDay(pricingConfigurations: PricingConfiguration[]): { key: string; value: PricingConfiguration[] }[] {
    const grouped: { [key: string]: PricingConfiguration[] } = {};
    
    pricingConfigurations?.forEach(config => {
      const dayName = this.daysOfWeek[parseInt(config.dayOfWeek)];
      if (!grouped[dayName]) {
        grouped[dayName] = [];
      }
      grouped[dayName].push(config);
    });

    // Sort each day's pricing by start time
    Object.keys(grouped).forEach(day => {
      grouped[day].sort((a, b) => a.startTime.localeCompare(b.startTime));
    });

    // Return in proper day order (Sunday to Saturday)
    return this.daysOfWeek
      .filter(day => grouped[day]) // Only include days that have pricing configurations
      .map(day => ({
        key: day,
        value: grouped[day]
      }));
  }

  formatTime(time: string): string {
    // Convert 24-hour format to 12-hour format
    const [hours, minutes] = time.split(':');
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour === 0 ? 12 : hour > 12 ? hour - 12 : hour;
    return `${displayHour}:${minutes} ${ampm}`;
  }

  formatPriceRange(prices: PricingConfiguration[]): string {
    const uniquePrices = [...new Set(prices.map(p => p.price))].sort((a, b) => a - b);
    if (uniquePrices.length === 1) {
      return `$${uniquePrices[0]}`;
    }
    return `$${uniquePrices[0]} - $${uniquePrices[uniquePrices.length - 1]}`;
  }

  getWorkingHours(pricingConfigurations: PricingConfiguration[]): string {
    if (!pricingConfigurations || pricingConfigurations.length === 0) {
      return 'No working hours set';
    }

    const times = pricingConfigurations.map(config => ({
      start: config.startTime,
      end: config.endTime
    }));

    times.sort((a, b) => a.start.localeCompare(b.start));

    const earliestStart = times[0].start;
    const latestEnd = times.reduce((latest, current) => 
      current.end > latest ? current.end : latest, times[0].end);

    return `${this.formatTime(earliestStart)} - ${this.formatTime(latestEnd)}`;
  }

  onMenuClick(event: Event, courtId: number): void {
    event.stopPropagation();
    this.activeMenuCourtId = this.activeMenuCourtId === courtId ? null : courtId;
  }

  onClickOutside(): void {
    this.activeMenuCourtId = null;
  }
} 