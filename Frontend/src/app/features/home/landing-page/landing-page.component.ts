import { Component } from '@angular/core';
import { Router } from '@angular/router';

interface Facility {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
}

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss']
})
export class LandingPageComponent {
  facilities: Facility[] = [
    {
      id: 1,
      name: 'Sports Center 1',
      description: 'Professional courts with top-notch facilities',
      imageUrl: 'assets/images/court-1.jpg'
    },
    {
      id: 2,
      name: 'Sports Center 2',
      description: 'Professional courts with top-notch facilities',
      imageUrl: 'assets/images/court-2.jpg'
    },
    {
      id: 3,
      name: 'Sports Center 3',
      description: 'Professional courts with top-notch facilities',
      imageUrl: 'assets/images/court-3.jpg'
    }
  ];

  constructor(private router: Router) {}

  navigateToBooking() {
    this.router.navigate(['/booking']);
  }

  viewDetails(facilityId: number) {
    this.router.navigate(['/facilities', facilityId]);
  }
}
