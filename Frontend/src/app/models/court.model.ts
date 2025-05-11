export interface PricingConfiguration {
  id: number;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  price: number;
  hourlyRate: number;
  isActive: boolean;
}

export interface PricingConfigurationRequest {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  price: number;
  hourlyRate: number;
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