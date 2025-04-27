import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Material Module
import { MaterialModule } from './material.module';

// PrimeNG Modules
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

const primeNgModules = [
  ButtonModule,
  InputTextModule,
  CalendarModule,
  DropdownModule,
  TableModule,
  DialogModule,
  ToastModule
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    ...primeNgModules
  ],
  exports: [
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    ...primeNgModules
  ],
  providers: [MessageService]
})
export class SharedModule { } 