import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar.component';
import { FooterComponent } from './components/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, FooterComponent], // Added FooterComponent
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss' // Corrected from styleUrls to styleUrl
})
export class AppComponent {
  title = 'badminton-booking';
}
