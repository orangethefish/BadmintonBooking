using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.API.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CourtId { get; set; }
        public Court Court { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        [Required]
        public decimal TotalPrice { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; }
        
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 