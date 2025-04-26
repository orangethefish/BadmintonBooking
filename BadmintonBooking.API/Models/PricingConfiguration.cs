using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class PricingConfiguration
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CourtId { get; set; }
        public Court Court { get; set; }
        
        [Required]
        public DayOfWeek DayOfWeek { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [Required]
        public decimal Price { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 