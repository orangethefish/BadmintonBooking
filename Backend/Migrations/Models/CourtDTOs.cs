using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class PricingConfigurationRequest
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        
        [Required]
        public string StartTime { get; set; }
        
        [Required]
        public string EndTime { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }
    }

    public class BatchCreateCourtRequest
    {
        [Required]
        [MaxLength(50)]
        public string BaseName { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int NumberOfCourts { get; set; }
        
        [Required]
        public int FacilityId { get; set; }
        
        [Required]
        public List<PricingConfigurationRequest> PricingConfigurations { get; set; }
    }

    public class UpdateCourtRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public List<PricingConfigurationRequest> PricingConfigurations { get; set; }
    }

    public class CourtResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FacilityId { get; set; }
        public Guid OwnerId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PricingConfigurationResponse> PricingConfigurations { get; set; }
    }

    public class PricingConfigurationResponse
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
} 