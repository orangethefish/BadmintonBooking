using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class BookingLock
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CourtId { get; set; }
        public Court Court { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        [Required]
        public string LockedBy { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
} 