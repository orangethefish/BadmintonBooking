export interface PricingConfiguration {
  id: number;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  price: number;
  isActive: boolean;
}

export interface PricingConfigurationRequest {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  price: number;
}

export interface PricingConfigurationFormData {
  daysOfWeek: number[];  // Multiple days of week in an array
  startTime: string;
  endTime: string;
  price: number;
}

export interface Court {
  id: number;
  name: string;
  facilityId: number;
  ownerId: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
  pricingConfigurations?: PricingConfiguration[];
}

export interface BatchCreateCourtRequest {
  baseName: string;
  numberOfCourts: number;
  facilityId: number;
  pricingConfigurations: PricingConfigurationRequest[];
} 