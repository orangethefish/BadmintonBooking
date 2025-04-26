using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class Court
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        [Required]
        public int FacilityId { get; set; }
        public Facility Facility { get; set; }
        
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<PricingConfiguration> PricingConfigurations { get; set; }
    }
} 