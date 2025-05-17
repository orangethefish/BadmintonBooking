export interface Facility {
  id: number;
  name: string;
  address: string;
  phoneNumber: string;
  description?: string;
  mapsUrl?: string;
  courtLatitude?: string;
  courtLongitude?: string;
  placeId?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface ResolveUrlResponse {
  finalUrl: string;
  formattedAddress?: string;
  name?: string;
  latitude?: number;
  longitude?: number;
  phoneNumber?: string;
  placeId?: string;
} 