using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class CreateFacilityRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        
        public string Description { get; set; }

        [StringLength(200)]
        public string MapsUrl { get; set; }
        
        [StringLength(100)]
        public string CourtLatitude { get; set; }
        
        [StringLength(100)]
        public string CourtLongitude { get; set; }
        
        [StringLength(36)]
        public string PlaceId { get; set; }
    }

    public class UpdateFacilityRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        
        public string Description { get; set; }
    }

    public class FacilityResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string MapsUrl { get; set; }
        public string CourtLatitude { get; set; }
        public string CourtLongitude { get; set; }
        public string PlaceId { get; set; }
    }

    public class ResolveUrlRequest
    {
        [Required]
        public string Url { get; set; }
    }

    public class ResolveUrlResponse
    {
        public string FinalUrl { get; set; }
        public string FormattedAddress { get; set; }
        public string Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; }
        public string PlaceId { get; set; }
    }

    public class PlaceInfo
    {
        public string FormattedAddress { get; set; }
        public string Name { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; }
        public string PlaceId { get; set; }
    }
} 